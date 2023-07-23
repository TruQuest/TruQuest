import 'dart:async';

import '../../user/services/user_service.dart';
import '../../general/contracts/acceptance_poll_contract.dart';
import '../../ethereum/errors/ethereum_error.dart';
import '../../general/extensions/datetime_extension.dart';
import '../../general/models/rvm/verifier_lottery_participant_entry_vm.dart';
import '../models/im/decision_im.dart';
import '../../general/contracts/assessment_poll_contract.dart';
import '../models/rvm/get_verifier_lottery_participants_rvm.dart';
import '../../ethereum/services/ethereum_service.dart';
import '../../general/contracts/thing_assessment_verifier_lottery_contract.dart';
import '../../general/contracts/truquest_contract.dart';
import '../models/rvm/get_settlement_proposal_rvm.dart';
import '../../general/contexts/document_context.dart';
import '../models/rvm/get_verifiers_rvm.dart';
import 'settlement_api_service.dart';

class SettlementService {
  final UserService _userService;
  final TruQuestContract _truQuestContract;
  final SettlementApiService _settlementApiService;
  final EthereumService _ethereumService;
  final AcceptancePollContract _acceptancePollContract;
  final ThingAssessmentVerifierLotteryContract
      _thingAssessmentVerifierLotteryContract;
  final AssessmentPollContract _assessmentPollContract;

  final StreamController<Stream<int>> _progress$Channel =
      StreamController<Stream<int>>();
  Stream<Stream<int>> get progress$$ => _progress$Channel.stream;

  SettlementService(
    this._userService,
    this._truQuestContract,
    this._settlementApiService,
    this._ethereumService,
    this._acceptancePollContract,
    this._thingAssessmentVerifierLotteryContract,
    this._assessmentPollContract,
  );

  Future createNewSettlementProposalDraft(
    DocumentContext documentContext,
  ) async {
    var progress$ =
        await _settlementApiService.createNewSettlementProposalDraft(
      documentContext.thingId!,
      documentContext.nameOrTitle!,
      documentContext.verdict!,
      documentContext.details!,
      documentContext.imageExt,
      documentContext.imageBytes,
      documentContext.croppedImageBytes,
      documentContext.evidence,
    );

    _progress$Channel.add(progress$);
  }

  Future<GetSettlementProposalRvm> getSettlementProposal(
    String proposalId,
  ) async {
    var result = await _settlementApiService.getSettlementProposal(proposalId);
    print('ProposalId: ${result.proposal.id}');
    return result;
  }

  Future<bool> checkThingAlreadyHasSettlementProposalUnderAssessment(
    String thingId,
  ) {
    return _truQuestContract
        .checkThingAlreadyHasSettlementProposalUnderAssessment(thingId);
  }

  Future submitNewSettlementProposal(String proposalId) async {
    await _settlementApiService.submitNewSettlementProposal(
      proposalId,
    );
  }

  Future fundSettlementProposal(
    String thingId,
    String proposalId,
    String signature,
  ) async {
    await _truQuestContract.fundThingSettlementProposal(
      thingId,
      proposalId,
      signature,
    );
  }

  Future<(String?, int?, int, int, bool?, bool?, int)> getVerifierLotteryInfo(
    String thingId,
    String proposalId,
  ) async {
    var currentUserId = _userService.latestCurrentUser?.id;
    int? initBlock =
        await _thingAssessmentVerifierLotteryContract.getLotteryInitBlock(
      thingId,
      proposalId,
    );
    if (initBlock == 0) {
      initBlock = null;
    }
    int durationBlocks = await _thingAssessmentVerifierLotteryContract
        .getLotteryDurationBlocks();

    // @@??: Should get here the currently connected account and then pass
    // it as an argument to the methods?

    // int thingVerifiersArrayIndex =
    // await _acceptancePollContract.getUserIndexAmongThingVerifiers(thingId);
    int thingVerifiersArrayIndex = 5;

    bool? alreadyClaimedASpot = await _thingAssessmentVerifierLotteryContract
        .checkAlreadyClaimedLotterySpot(
      thingId,
      proposalId,
    );

    bool? alreadyJoined =
        await _thingAssessmentVerifierLotteryContract.checkAlreadyJoinedLottery(
      thingId,
      proposalId,
    );

    int latestBlockNumber = await _ethereumService.getLatestL1BlockNumber();

    return (
      currentUserId,
      initBlock,
      durationBlocks,
      thingVerifiersArrayIndex,
      alreadyClaimedASpot,
      alreadyJoined,
      latestBlockNumber,
    );
  }

  Future<EthereumError?> claimLotterySpot(
    String thingId,
    String proposalId,
    int userIndexInThingVerifiersArray,
  ) async {
    var error = await _thingAssessmentVerifierLotteryContract.claimLotterySpot(
      thingId,
      proposalId,
      userIndexInThingVerifiersArray,
    );
    return error;
  }

  Future<EthereumError?> joinLottery(String thingId, String proposalId) async {
    var error = await _thingAssessmentVerifierLotteryContract.joinLottery(
      thingId,
      proposalId,
    );
    return error;
  }

  Future<GetVerifierLotteryParticipantsRvm> getVerifierLotteryParticipants(
    String thingId,
    String proposalId,
  ) async {
    var result = await _settlementApiService.getVerifierLotteryParticipants(
      proposalId,
    );

    var (lotteryInitBlock, dataHash, userXorDataHash) =
        await _thingAssessmentVerifierLotteryContract.getOrchestratorCommitment(
      thingId,
      proposalId,
    );

    var entries = result.entries;
    if (entries.isEmpty || !entries.first.isOrchestrator) {
      if (lotteryInitBlock != 0) {
        result = GetVerifierLotteryParticipantsRvm(
          proposalId: proposalId,
          entries: List.unmodifiable([
            VerifierLotteryParticipantEntryVm.orchestratorNoNonce(
              lotteryInitBlock.abs(),
              dataHash,
              userXorDataHash,
            ),
            ...entries,
          ]),
        );
      }
    } else {
      result = GetVerifierLotteryParticipantsRvm(
        proposalId: proposalId,
        entries: List.unmodifiable(
          [
            entries.first.copyWith(
              'Orchestrator',
              dataHash,
              userXorDataHash,
            ),
            ...entries.skip(1)
          ],
        ),
      );
    }

    return result;
  }

  Future<(String?, int?, int, int, int)> getAssessmentPollInfo(
    String thingId,
    String proposalId,
  ) async {
    var currentUserId = _userService.latestCurrentUser?.id;
    int? initBlock = await _assessmentPollContract.getPollInitBlock(
      thingId,
      proposalId,
    );
    if (initBlock == 0) {
      initBlock = null;
    }
    int durationBlocks = await _assessmentPollContract.getPollDurationBlocks();
    int proposalVerifiersArrayIndex =
        await _assessmentPollContract.getUserIndexAmongProposalVerifiers(
      thingId,
      proposalId,
    );
    int latestBlockNumber = await _ethereumService.getLatestL1BlockNumber();

    return (
      currentUserId,
      initBlock,
      durationBlocks,
      proposalVerifiersArrayIndex,
      latestBlockNumber,
    );
  }

  Future castVoteOffChain(
    String thingId,
    String proposalId,
    DecisionIm decision,
    String reason,
  ) async {
    var castedAt = DateTime.now().getString();
    var result =
        await _ethereumService.signThingSettlementProposalAssessmentPollVote(
      thingId,
      proposalId,
      castedAt,
      decision,
      reason,
    );
    if (result.isLeft) {
      print(result.left);
      return;
    }

    var ipfsCid = await _settlementApiService
        .castThingSettlementProposalAssessmentPollVote(
      thingId,
      proposalId,
      castedAt,
      decision,
      reason,
      result.right,
    );

    print('Vote cid: $ipfsCid');
  }

  Future castVoteOnChain(
    String thingId,
    String proposalId,
    int userIndexInProposalVerifiersArray,
    DecisionIm decision,
    String reason,
  ) async {
    await _assessmentPollContract.castVote(
      thingId,
      proposalId,
      userIndexInProposalVerifiersArray,
      decision,
      reason,
    );
  }

  Future<GetVerifiersRvm> getVerifiers(String proposalId) async {
    var result = await _settlementApiService.getVerifiers(proposalId);
    return result;
  }
}
