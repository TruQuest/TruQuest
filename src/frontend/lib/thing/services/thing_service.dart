import 'dart:async';

import 'package:either_dart/either.dart';

import '../../user/services/user_service.dart';
import '../../general/models/rvm/verifier_lottery_participant_entry_vm.dart';
import '../../ethereum/errors/ethereum_error.dart';
import '../errors/thing_error.dart';
import '../models/rvm/get_settlement_proposals_list_rvm.dart';
import '../models/rvm/get_verifiers_rvm.dart';
import '../../general/extensions/datetime_extension.dart';
import '../../general/contracts/acceptance_poll_contract.dart';
import '../../ethereum/services/ethereum_service.dart';
import '../../general/contracts/thing_submission_verifier_lottery_contract.dart';
import '../models/im/decision_im.dart';
import '../models/rvm/get_thing_rvm.dart';
import '../../general/contracts/truquest_contract.dart';
import '../../general/contexts/document_context.dart';
import '../models/rvm/get_verifier_lottery_participants_rvm.dart';
import '../models/rvm/submit_new_thing_rvm.dart';
import 'thing_api_service.dart';

class ThingService {
  final ThingApiService _thingApiService;
  final UserService _userService;
  final EthereumService _ethereumService;
  final TruQuestContract _truQuestContract;
  final ThingSubmissionVerifierLotteryContract
      _thingSubmissionVerifierLotteryContract;
  final AcceptancePollContract _acceptancePollContract;

  final StreamController<Stream<int>> _progress$Channel =
      StreamController<Stream<int>>();
  Stream<Stream<int>> get progress$$ => _progress$Channel.stream;

  ThingService(
    this._thingApiService,
    this._userService,
    this._ethereumService,
    this._truQuestContract,
    this._thingSubmissionVerifierLotteryContract,
    this._acceptancePollContract,
  );

  Future createNewThingDraft(DocumentContext documentContext) async {
    var progress$ = await _thingApiService.createNewThingDraft(
      documentContext.subjectId!,
      documentContext.nameOrTitle!,
      documentContext.details!,
      documentContext.imageExt,
      documentContext.imageBytes,
      documentContext.croppedImageBytes,
      documentContext.evidence,
      documentContext.tags,
    );

    _progress$Channel.add(progress$);
  }

  Future<Either<ThingError, GetThingRvm>> getThing(String thingId) async {
    try {
      var result = await _thingApiService.getThing(thingId);
      print('ThingId: ${result.thing.id}');
      return Right(result);
    } on ThingError catch (e) {
      print(e);
      return Left(e);
    }
  }

  Future<bool> checkThingAlreadyFunded(String thingId) {
    return _truQuestContract.checkThingAlreadyFunded(thingId);
  }

  Future<SubmitNewThingRvm> submitNewThing(String thingId) async {
    var result = await _thingApiService.submitNewThing(thingId);
    return result;
  }

  Future fundThing(String thingId, String signature) async {
    await _truQuestContract.fundThing(thingId, signature);
  }

  Future<(String?, int?, int, bool?, int)> getVerifierLotteryInfo(
    String thingId,
  ) async {
    var currentUserId = _userService.latestCurrentUser?.id;
    int? initBlock = await _thingSubmissionVerifierLotteryContract
        .getLotteryInitBlock(thingId);
    if (initBlock == 0) {
      initBlock = null;
    }
    int durationBlocks = await _thingSubmissionVerifierLotteryContract
        .getLotteryDurationBlocks();
    bool? alreadyJoined = await _thingSubmissionVerifierLotteryContract
        .checkAlreadyJoinedLottery(thingId);
    int latestL1BlockNumber = await _ethereumService.getLatestL1BlockNumber();

    return (
      currentUserId,
      initBlock,
      durationBlocks,
      alreadyJoined,
      latestL1BlockNumber,
    );
  }

  Future<EthereumError?> joinLottery(String thingId) async {
    var error = await _thingSubmissionVerifierLotteryContract.joinLottery(
      thingId,
    );
    return error;
  }

  Future<GetVerifierLotteryParticipantsRvm> getVerifierLotteryParticipants(
    String thingId,
  ) async {
    var result = await _thingApiService.getVerifierLotteryParticipants(thingId);

    var nullString = '0x${List.generate(64, (_) => '0').join()}';
    // @@TODO: Query all at once.
    var lotteryInitBlock = await _thingSubmissionVerifierLotteryContract
        .getLotteryInitBlock(thingId);
    var dataHash = await _thingSubmissionVerifierLotteryContract
            .getOrchestratorCommitmentDataHash(thingId) ??
        nullString;
    var userXorDataHash = await _thingSubmissionVerifierLotteryContract
            .getOrchestratorCommitmentUserXorDataHash(thingId) ??
        nullString;

    var entries = result.entries;
    if (entries.isEmpty || !entries.first.isOrchestrator) {
      if (lotteryInitBlock != 0) {
        result = GetVerifierLotteryParticipantsRvm(
          thingId: thingId,
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
        thingId: thingId,
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

  Future<(String?, int?, int, int, int)> getAcceptancePollInfo(
    String thingId,
  ) async {
    var currentUserId = _userService.latestCurrentUser?.id;
    int? initBlock = await _acceptancePollContract.getPollInitBlock(thingId);
    if (initBlock == 0) {
      initBlock = null;
    }
    int durationBlocks = await _acceptancePollContract.getPollDurationBlocks();
    int thingVerifiersArrayIndex =
        await _acceptancePollContract.getUserIndexAmongThingVerifiers(thingId);
    int latestBlockNumber = await _ethereumService.getLatestL1BlockNumber();

    return (
      currentUserId,
      initBlock,
      durationBlocks,
      thingVerifiersArrayIndex,
      latestBlockNumber,
    );
  }

  Future castVoteOffChain(
    String thingId,
    DecisionIm decision,
    String reason,
  ) async {
    var castedAt = DateTime.now().getString();
    var result = await _ethereumService.signThingAcceptancePollVote(
      thingId,
      castedAt,
      decision,
      reason,
    );
    if (result.isLeft) {
      print(result.left);
      return;
    }

    var ipfsCid = await _thingApiService.castThingAcceptancePollVote(
      thingId,
      castedAt,
      decision,
      reason,
      result.right,
    );

    print('Vote cid: $ipfsCid');
  }

  Future castVoteOnChain(
    String thingId,
    int userIndexInThingVerifiersArray,
    DecisionIm decision,
    String reason,
  ) async {
    await _acceptancePollContract.castVote(
      thingId,
      userIndexInThingVerifiersArray,
      decision,
      reason,
    );
  }

  Future<GetVerifiersRvm> getVerifiers(String thingId) async {
    var result = await _thingApiService.getVerifiers(thingId);
    return result;
  }

  Future<GetSettlementProposalsListRvm> getSettlementProposalsList(
    String thingId,
  ) async {
    var result = await _thingApiService.getSettlementProposalsList(thingId);
    return result;
  }

  Future watch(String thingId, bool markedAsWatched) async {
    await _thingApiService.watch(thingId, markedAsWatched);
  }
}
