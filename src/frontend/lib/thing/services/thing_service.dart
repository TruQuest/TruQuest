import 'dart:async';

import 'package:tuple/tuple.dart';

import '../../ethereum/services/ethereum_service.dart';
import '../../general/contracts/thing_submission_verifier_lottery_contract.dart';
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

  final StreamController<Stream<int>> _progress$Channel =
      StreamController<Stream<int>>();
  Stream<Stream<int>> get progress$$ => _progress$Channel.stream;

  ThingService(
    this._thingApiService,
    this._ethereumService,
    this._truQuestContract,
    this._thingSubmissionVerifierLotteryContract,
  );

  Future<Stream<int>> createNewThingDraft(
    DocumentContext documentContext,
  ) async {
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
    return progress$;
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
    print(result.thingId);
    print(result.signature);
    return result;
  }

  Future fundThing(String thingId, String signature) async {
    await _truQuestContract.fundThing(thingId, signature);
  }

  Future<Tuple5<int?, int, bool?, bool?, int>> getVerifierLotteryInfo(
    String thingId,
  ) async {
    int? lotteryInitBlock = await _thingSubmissionVerifierLotteryContract
        .getLotteryInitBlock(thingId);
    if (lotteryInitBlock == 0) {
      lotteryInitBlock = null;
    }
    int lotteryDurationBlocks = await _thingSubmissionVerifierLotteryContract
        .getLotteryDurationBlocks();
    bool? alreadyPreJoinedLottery =
        await _thingSubmissionVerifierLotteryContract
            .checkAlreadyPreJoinedLottery(thingId);
    bool? alreadyJoinedLottery = await _thingSubmissionVerifierLotteryContract
        .checkAlreadyJoinedLottery(thingId);
    int latestBlockNumber = await _ethereumService.getLatestBlockNumber();

    return Tuple5(
      lotteryInitBlock,
      lotteryDurationBlocks,
      alreadyPreJoinedLottery,
      alreadyJoinedLottery,
      latestBlockNumber,
    );
  }

  Future preJoinLottery(String thingId) async {
    await _thingSubmissionVerifierLotteryContract.preJoinLottery(thingId);
  }

  Future joinLottery(String thingId) async {
    await _thingSubmissionVerifierLotteryContract.joinLottery(thingId);
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
}
