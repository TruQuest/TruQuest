import 'dart:async';

import 'package:tuple/tuple.dart';

import '../../general/extensions/datetime_extension.dart';
import '../models/im/decision_im.dart';
import '../../general/contracts/assessment_poll_contract.dart';
import '../models/rvm/get_verifier_lottery_participants_rvm.dart';
import '../../ethereum/services/ethereum_service.dart';
import '../../general/contracts/thing_assessment_verifier_lottery_contract.dart';
import '../../general/contracts/truquest_contract.dart';
import '../models/rvm/get_settlement_proposal_rvm.dart';
import '../models/rvm/get_settlement_proposals_rvm.dart';
import '../../general/contexts/document_context.dart';
import '../models/rvm/get_verifiers_rvm.dart';
import 'settlement_api_service.dart';

class SettlementService {
  final TruQuestContract _truQuestContract;
  final SettlementApiService _settlementApiService;
  final EthereumService _ethereumService;
  final ThingAssessmentVerifierLotteryContract
      _thingAssessmentVerifierLotteryContract;
  final AssessmentPollContract _assessmentPollContract;

  final StreamController<Stream<int>> _progress$Channel =
      StreamController<Stream<int>>();
  Stream<Stream<int>> get progress$$ => _progress$Channel.stream;

  SettlementService(
    this._truQuestContract,
    this._settlementApiService,
    this._ethereumService,
    this._thingAssessmentVerifierLotteryContract,
    this._assessmentPollContract,
  );

  Future<GetSettlementProposalsRvm> getSettlementProposalsFor(
    String thingId,
  ) async {
    var result = await _settlementApiService.getSettlementProposalsFor(thingId);
    return result;
  }

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

  Future subscribeToProposal(String proposalId) async {
    await _settlementApiService.subscribeToProposal(proposalId);
  }

  Future unsubscribeFromProposal(String proposalId) async {
    await _settlementApiService.unsubscribeFromProposal(proposalId);
  }

  Future<bool> checkThingAlreadyHasSettlementProposalUnderAssessment(
    String thingId,
  ) {
    return _truQuestContract
        .checkThingAlreadyHasSettlementProposalUnderAssessment(thingId);
  }

  Future submitNewSettlementProposal(String proposalId) async {
    var result = await _settlementApiService.submitNewSettlementProposal(
      proposalId,
    );
    print(result.thingId);
    print(result.proposalId);
    print(result.signature);
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

  Future<Tuple5<int?, int, bool?, bool?, int>> getVerifierLotteryInfo(
    String thingId,
    String proposalId,
  ) async {
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
    bool? alreadyPreJoined = await _thingAssessmentVerifierLotteryContract
        .checkAlreadyPreJoinedLottery(
      thingId,
      proposalId,
    );
    bool? alreadyJoined =
        await _thingAssessmentVerifierLotteryContract.checkAlreadyJoinedLottery(
      thingId,
      proposalId,
    );
    int latestBlockNumber = await _ethereumService.getLatestBlockNumber();

    return Tuple5(
      initBlock,
      durationBlocks,
      alreadyPreJoined,
      alreadyJoined,
      latestBlockNumber,
    );
  }

  Future claimLotterySpot(String thingId, String proposalId) async {
    await _thingAssessmentVerifierLotteryContract.claimLotterySpot(
      thingId,
      proposalId,
    );
  }

  Future preJoinLottery(String thingId, String proposalId) async {
    await _thingAssessmentVerifierLotteryContract.preJoinLottery(
      thingId,
      proposalId,
    );
  }

  Future joinLottery(String thingId, String proposalId) async {
    await _thingAssessmentVerifierLotteryContract.joinLottery(
      thingId,
      proposalId,
    );
  }

  Future<GetVerifierLotteryParticipantsRvm> getVerifierLotteryParticipants(
    String proposalId,
  ) async {
    var result = await _settlementApiService.getVerifierLotteryParticipants(
      proposalId,
    );
    return result;
  }

  Future<Tuple4<int?, int, bool?, int>> getAssessmentPollInfo(
    String thingId,
    String proposalId,
  ) async {
    int? initBlock = await _assessmentPollContract.getPollInitBlock(
      thingId,
      proposalId,
    );
    if (initBlock == 0) {
      initBlock = null;
    }
    int durationBlocks = await _assessmentPollContract.getPollDurationBlocks();
    bool? isDesignatedVerifier =
        await _assessmentPollContract.checkIsDesignatedVerifierForProposal(
      thingId,
      proposalId,
    );
    int latestBlockNumber = await _ethereumService.getLatestBlockNumber();

    return Tuple4(
      initBlock,
      durationBlocks,
      isDesignatedVerifier,
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
    DecisionIm decision,
    String reason,
  ) async {
    await _assessmentPollContract.castVote(
      thingId,
      proposalId,
      decision,
      reason,
    );
  }

  Future<GetVerifiersRvm> getVerifiers(String proposalId) async {
    var result = await _settlementApiService.getVerifiers(proposalId);
    return result;
  }
}
