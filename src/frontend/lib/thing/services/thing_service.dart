import 'dart:async';

import 'package:tuple/tuple.dart';

import '../../ethereum/errors/ethereum_error.dart';
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
      [1, 2, 3],
    );

    _progress$Channel.add(progress$);
  }

  Future<GetThingRvm> getThing(String thingId) async {
    var result = await _thingApiService.getThing(thingId);
    print('ThingId: ${result.thing.id}');
    return result;
  }

  Future subscribeToThing(String thingId) async {
    await _thingApiService.subscribeToThing(thingId);
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

  Future<Tuple5<int?, int, bool?, bool?, int>> getVerifierLotteryInfo(
    String thingId,
  ) async {
    int? initBlock = await _thingSubmissionVerifierLotteryContract
        .getLotteryInitBlock(thingId);
    if (initBlock == 0) {
      initBlock = null;
    }
    int durationBlocks = await _thingSubmissionVerifierLotteryContract
        .getLotteryDurationBlocks();
    bool? alreadyPreJoined = await _thingSubmissionVerifierLotteryContract
        .checkAlreadyPreJoinedLottery(thingId);
    bool? alreadyJoined = await _thingSubmissionVerifierLotteryContract
        .checkAlreadyJoinedLottery(thingId);
    int latestBlockNumber = await _ethereumService.getLatestBlockNumber();

    return Tuple5(
      initBlock,
      durationBlocks,
      alreadyPreJoined,
      alreadyJoined,
      latestBlockNumber,
    );
  }

  Future<EthereumError?> preJoinLottery(String thingId) async {
    var error = await _thingSubmissionVerifierLotteryContract.preJoinLottery(
      thingId,
    );
    return error;
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
    return result;
  }

  Future unsubscribeFromThing(String thingId) async {
    await _thingApiService.unsubscribeFromThing(thingId);
  }

  Future<Tuple4<int?, int, bool?, int>> getAcceptancePollInfo(
    String thingId,
  ) async {
    int? initBlock = await _acceptancePollContract.getPollInitBlock(thingId);
    if (initBlock == 0) {
      initBlock = null;
    }
    int durationBlocks = await _acceptancePollContract.getPollDurationBlocks();
    bool? isDesignatedVerifier = await _acceptancePollContract
        .checkIsDesignatedVerifierForThing(thingId);
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
    DecisionIm decision,
    String reason,
  ) async {
    await _acceptancePollContract.castVote(thingId, decision, reason);
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
}
