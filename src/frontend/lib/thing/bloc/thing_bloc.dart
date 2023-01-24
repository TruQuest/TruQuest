import 'dart:async';

import '../models/rvm/get_verifiers_rvm.dart';
import '../models/rvm/thing_state_vm.dart';
import '../models/rvm/get_thing_rvm.dart';
import 'thing_result_vm.dart';
import '../../general/bloc/bloc.dart';
import '../services/thing_service.dart';
import 'thing_actions.dart';

class ThingBloc extends Bloc<ThingAction> {
  final ThingService _thingService;

  final StreamController<GetThingRvm> _thingChannel =
      StreamController<GetThingRvm>.broadcast();
  Stream<GetThingRvm> get thing$ => _thingChannel.stream;

  final StreamController<GetVerifierLotteryInfoSuccessVm>
      _verifierLotteryInfoChannel =
      StreamController<GetVerifierLotteryInfoSuccessVm>.broadcast();
  Stream<GetVerifierLotteryInfoSuccessVm> get verifierLotteryInfo$ =>
      _verifierLotteryInfoChannel.stream;

  final StreamController<GetVerifierLotteryParticipantsSuccessVm>
      _verifierLotteryParticipantsChannel =
      StreamController<GetVerifierLotteryParticipantsSuccessVm>.broadcast();
  Stream<GetVerifierLotteryParticipantsSuccessVm>
      get verifierLotteryParticipants$ =>
          _verifierLotteryParticipantsChannel.stream;

  final StreamController<GetVerifiersRvm> _verifiersChannel =
      StreamController<GetVerifiersRvm>.broadcast();
  Stream<GetVerifiersRvm> get verifiers$ => _verifiersChannel.stream;

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
      } else if (action is GetVerifierLotteryInfo) {
        _getVerifierLotteryInfo(action);
      } else if (action is PreJoinLottery) {
        _preJoinLottery(action);
      } else if (action is JoinLottery) {
        _joinLottery(action);
      } else if (action is GetVerifierLotteryParticipants) {
        _getVerifierLotteryParticipants(action);
      } else if (action is UnsubscribeFromThing) {
        _unsubscribeFromThing(action);
      } else if (action is GetAcceptancePollInfo) {
        _getAcceptancePollInfo(action);
      } else if (action is CastVoteOffChain) {
        _castVoteOffChain(action);
      } else if (action is CastVoteOnChain) {
        _castVoteOnChain(action);
      } else if (action is GetVerifiers) {
        _getVerifiers(action);
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
    if (action.subscribe) {
      await _thingService.subscribeToThing(action.thingId);
    }

    if (result.thing.state == ThingStateVm.awaitingFunding) {
      bool thingFunded =
          await _thingService.checkThingAlreadyFunded(action.thingId);
      result = GetThingRvm(
        thing: result.thing.copyWith(
          fundedAwaitingConfirmation: thingFunded,
        ),
        signature: result.signature,
      );
    }

    _thingChannel.add(result);
  }

  void _submitNewThing(SubmitNewThing action) async {
    var result = await _thingService.submitNewThing(action.thing.id);
    _thingChannel.add(GetThingRvm(
      thing: action.thing.copyWith(
        state: ThingStateVm.awaitingFunding,
        fundedAwaitingConfirmation: false,
      ),
      signature: result.signature,
    ));
  }

  void _fundThing(FundThing action) async {
    await _thingService.fundThing(action.thing.id, action.signature);
    _thingChannel.add(GetThingRvm(
      thing: action.thing.copyWith(
        // state: ThingStateVm.awaitingFunding,
        fundedAwaitingConfirmation: true,
      ),
      signature: null,
    ));
  }

  void _getVerifierLotteryInfo(GetVerifierLotteryInfo action) async {
    var info = await _thingService.getVerifierLotteryInfo(action.thingId);
    _verifierLotteryInfoChannel.add(
      GetVerifierLotteryInfoSuccessVm(
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

    var info = await _thingService.getVerifierLotteryInfo(action.thingId);
    _verifierLotteryInfoChannel.add(
      GetVerifierLotteryInfoSuccessVm(
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

    var info = await _thingService.getVerifierLotteryInfo(action.thingId);
    _verifierLotteryInfoChannel.add(
      GetVerifierLotteryInfoSuccessVm(
        initBlock: info.item1,
        durationBlocks: info.item2,
        alreadyPreJoined: info.item3,
        alreadyJoined: info.item4,
        latestBlockNumber: info.item5,
      ),
    );
  }

  void _getVerifierLotteryParticipants(
    GetVerifierLotteryParticipants action,
  ) async {
    var result = await _thingService.getVerifierLotteryParticipants(
      action.thingId,
    );
    _verifierLotteryParticipantsChannel.add(
      GetVerifierLotteryParticipantsSuccessVm(entries: result.entries),
    );
  }

  void _unsubscribeFromThing(UnsubscribeFromThing action) async {
    await _thingService.unsubscribeFromThing(action.thingId);
  }

  void _getAcceptancePollInfo(GetAcceptancePollInfo action) async {
    var info = await _thingService.getAcceptancePollInfo(action.thingId);
    action.complete(
      GetAcceptancePollInfoSuccessVm(
        initBlock: info.item1,
        durationBlocks: info.item2,
        isDesignatedVerifier: info.item3,
        latestBlockNumber: info.item4,
      ),
    );
  }

  void _castVoteOffChain(CastVoteOffChain action) async {
    await _thingService.castVoteOffChain(
      action.thingId,
      action.decision,
      action.reason,
    );
  }

  void _castVoteOnChain(CastVoteOnChain action) async {
    await _thingService.castVoteOnChain(
      action.thingId,
      action.decision,
      action.reason,
    );
  }

  void _getVerifiers(GetVerifiers action) async {
    var result = await _thingService.getVerifiers(action.thingId);
    _verifiersChannel.add(result);
  }
}
