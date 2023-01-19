import 'dart:async';

import '../models/rvm/thing_state_vm.dart';
import '../models/rvm/get_thing_result_vm.dart';
import 'thing_result_vm.dart';
import '../../general/bloc/bloc.dart';
import '../services/thing_service.dart';
import 'thing_actions.dart';

class ThingBloc extends Bloc<ThingAction> {
  final ThingService _thingService;

  final StreamController<GetThingResultVm> _thingChannel =
      StreamController<GetThingResultVm>.broadcast();
  Stream<GetThingResultVm> get thing$ => _thingChannel.stream;

  final StreamController<GetThingLotteryInfoSuccessVm>
      _thingLotteryInfoChannel =
      StreamController<GetThingLotteryInfoSuccessVm>.broadcast();
  Stream<GetThingLotteryInfoSuccessVm> get thingLotteryInfo$ =>
      _thingLotteryInfoChannel.stream;

  ThingBloc(this._thingService) {
    actionChannel.stream.listen((action) {
      if (action is CreateNewThingDraft) {
        _createNewThingDraft(action);
      } else if (action is GetThing) {
        _getThing(action);
      } else if (action is SubmitNewThing) {
        _submitNewThing(action);
      } else if (action is FundThing) {
        _fundThing(action);
      } else if (action is GetThingLotteryInfo) {
        _getThingLotteryInfo(action);
      } else if (action is PreJoinLottery) {
        _preJoinLottery(action);
      } else if (action is JoinLottery) {
        _joinLottery(action);
      }
    });
  }

  @override
  void dispose({ThingAction? cleanupAction}) {
    // TODO: implement dispose
  }

  void _createNewThingDraft(CreateNewThingDraft action) async {
    await _thingService.createNewThingDraft(action.documentContext);
    action.complete(CreateNewThingDraftSuccessVm());
  }

  void _getThing(GetThing action) async {
    var result = await _thingService.getThing(action.thingId);
    _thingChannel.add(result);
  }

  void _submitNewThing(SubmitNewThing action) async {
    var result = await _thingService.submitNewThing(action.thing.id);
    _thingChannel.add(GetThingResultVm(
      thing: action.thing.copyWithNewState(ThingStateVm.awaitingFunding),
      signature: result.signature,
    ));
  }

  void _fundThing(FundThing action) async {
    await _thingService.fundThing(action.thing.id, action.signature);
    _thingChannel.add(GetThingResultVm(
      thing: action.thing.copyWithNewState(
        ThingStateVm.fundedAndSubmissionVerifierLotteryInitiated,
      ),
      signature: null,
    ));
  }

  void _getThingLotteryInfo(GetThingLotteryInfo action) async {
    var info = await _thingService.getThingLotteryInfo(action.thingId);
    _thingLotteryInfoChannel.add(
      GetThingLotteryInfoSuccessVm(
        initBlock: info.item1,
        durationBlocks: info.item2,
        alreadyPreJoined: info.item3,
        alreadyJoined: info.item4,
        latestBlockNumber: info.item5,
      ),
    );
  }

  void _preJoinLottery(PreJoinLottery action) async {
    await _thingService.preJoinLottery(action.thingId);

    var info = await _thingService.getThingLotteryInfo(action.thingId);
    _thingLotteryInfoChannel.add(
      GetThingLotteryInfoSuccessVm(
        initBlock: info.item1,
        durationBlocks: info.item2,
        alreadyPreJoined: info.item3,
        alreadyJoined: info.item4,
        latestBlockNumber: info.item5,
      ),
    );
  }

  void _joinLottery(JoinLottery action) async {
    await _thingService.joinLottery(action.thingId);

    var info = await _thingService.getThingLotteryInfo(action.thingId);
    _thingLotteryInfoChannel.add(
      GetThingLotteryInfoSuccessVm(
        initBlock: info.item1,
        durationBlocks: info.item2,
        alreadyPreJoined: info.item3,
        alreadyJoined: info.item4,
        latestBlockNumber: info.item5,
      ),
    );
  }
}
