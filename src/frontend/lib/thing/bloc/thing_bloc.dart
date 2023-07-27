import 'dart:async';

import '../../general/models/rvm/verifier_lottery_participant_entry_vm.dart';
import '../../general/models/rvm/verifier_vm.dart';
import '../../general/contexts/multi_stage_operation_context.dart';
import '../models/rvm/acceptance_poll_info_vm.dart';
import '../models/rvm/settlement_proposal_preview_vm.dart';
import '../models/rvm/thing_state_vm.dart';
import '../models/rvm/get_thing_rvm.dart';
import '../models/rvm/verifier_lottery_info_vm.dart';
import '../../general/bloc/bloc.dart';
import '../services/thing_service.dart';
import 'thing_actions.dart';

class ThingBloc extends Bloc<ThingAction> {
  final ThingService _thingService;

  final _thingChannel = StreamController<GetThingRvm?>.broadcast();
  Stream<GetThingRvm?> get thing$ => _thingChannel.stream;

  final _verifierLotteryInfoChannel = StreamController<VerifierLotteryInfoVm>.broadcast();
  Stream<VerifierLotteryInfoVm> get verifierLotteryInfo$ => _verifierLotteryInfoChannel.stream;

  final _verifierLotteryParticipantsChannel = StreamController<List<VerifierLotteryParticipantEntryVm>>.broadcast();
  Stream<List<VerifierLotteryParticipantEntryVm>> get verifierLotteryParticipants$ =>
      _verifierLotteryParticipantsChannel.stream;

  final _verifiersChannel = StreamController<List<VerifierVm>>.broadcast();
  Stream<List<VerifierVm>> get verifiers$ => _verifiersChannel.stream;

  final _proposalsListChannel = StreamController<List<SettlementProposalPreviewVm>>.broadcast();
  Stream<List<SettlementProposalPreviewVm>> get proposalsList$ => _proposalsListChannel.stream;

  ThingBloc(super.toastMessenger, this._thingService) {
    actionChannel.stream.listen((action) {
      if (action is GetThing) {
        _getThing(action);
      } else if (action is GetVerifierLotteryInfo) {
        _getVerifierLotteryInfo(action);
      } else if (action is GetVerifierLotteryParticipants) {
        _getVerifierLotteryParticipants(action);
      } else if (action is GetVerifiers) {
        _getVerifiers(action);
      } else if (action is GetSettlementProposalsList) {
        _getSettlementProposalsList(action);
      } else if (action is Watch) {
        _watch(action);
      }
    });
  }

  @override
  Future<Object?> handleExecute(ThingAction action) {
    if (action is CreateNewThingDraft) {
      return _createNewThingDraft(action);
    } else if (action is SubmitNewThing) {
      return _submitNewThing(action);
    } else if (action is GetAcceptancePollInfo) {
      return _getAcceptancePollInfo(action);
    }

    throw UnimplementedError();
  }

  @override
  Stream<Object> handleMultiStageExecute(
    ThingAction action,
    MultiStageOperationContext ctx,
  ) {
    if (action is FundThing) {
      return _fundThing(action, ctx);
    } else if (action is JoinLottery) {
      return _joinLottery(action, ctx);
    } else if (action is CastVoteOffChain) {
      return _castVoteOffChain(action, ctx);
    } else if (action is CastVoteOnChain) {
      return _castVoteOnChain(action, ctx);
    }

    throw UnimplementedError();
  }

  Future<bool> _createNewThingDraft(CreateNewThingDraft action) async {
    await _thingService.createNewThingDraft(action.documentContext);
    return true;
  }

  void _getThing(GetThing action) async {
    var result = await _thingService.getThing(action.thingId);
    if (result.isLeft) {
      _thingChannel.add(null);
      return;
    }

    var getThingResult = result.right;
    if (getThingResult.thing.state == ThingStateVm.awaitingFunding) {
      bool thingFunded = await _thingService.checkThingAlreadyFunded(
        action.thingId,
      );
      getThingResult = GetThingRvm(
        thing: getThingResult.thing.copyWith(
          fundedAwaitingConfirmation: thingFunded,
        ),
        signature: getThingResult.signature,
      );
    }

    _thingChannel.add(getThingResult);
  }

  Future<bool> _submitNewThing(SubmitNewThing action) async {
    await _thingService.submitNewThing(action.thingId);
    _getThing(GetThing(thingId: action.thingId));
    return true;
  }

  Stream<Object> _fundThing(FundThing action, MultiStageOperationContext ctx) =>
      _thingService.fundThing(action.thingId, action.signature, ctx);

  void _refreshVerifierLotteryInfo(String thingId) async {
    var info = await _thingService.getVerifierLotteryInfo(thingId);
    _verifierLotteryInfoChannel.add(
      VerifierLotteryInfoVm(
        userId: info.$1,
        initBlock: info.$2,
        durationBlocks: info.$3,
        alreadyJoined: info.$4,
      ),
    );
  }

  void _getVerifierLotteryInfo(GetVerifierLotteryInfo action) {
    _refreshVerifierLotteryInfo(action.thingId);
  }

  Stream<Object> _joinLottery(JoinLottery action, MultiStageOperationContext ctx) =>
      _thingService.joinLottery(action.thingId, ctx);

  void _getVerifierLotteryParticipants(
    GetVerifierLotteryParticipants action,
  ) async {
    var result = await _thingService.getVerifierLotteryParticipants(
      action.thingId,
    );
    _verifierLotteryParticipantsChannel.add(result.entries);
  }

  Future<AcceptancePollInfoVm> _getAcceptancePollInfo(GetAcceptancePollInfo action) async {
    var info = await _thingService.getAcceptancePollInfo(action.thingId);
    return AcceptancePollInfoVm(
      userId: info.$1,
      initBlock: info.$2,
      durationBlocks: info.$3,
      userIndexInThingVerifiersArray: info.$4,
    );
  }

  Stream<Object> _castVoteOffChain(CastVoteOffChain action, MultiStageOperationContext ctx) =>
      _thingService.castVoteOffChain(
        action.thingId,
        action.decision,
        action.reason,
        ctx,
      );

  Stream<Object> _castVoteOnChain(CastVoteOnChain action, MultiStageOperationContext ctx) =>
      _thingService.castVoteOnChain(
        action.thingId,
        action.userIndexInThingVerifiersArray,
        action.decision,
        action.reason,
        ctx,
      );

  void _getVerifiers(GetVerifiers action) async {
    var result = await _thingService.getVerifiers(action.thingId);
    _verifiersChannel.add(result.verifiers);
  }

  void _getSettlementProposalsList(GetSettlementProposalsList action) async {
    var result = await _thingService.getSettlementProposalsList(action.thingId);
    _proposalsListChannel.add(result.proposals);
  }

  void _watch(Watch action) async {
    await _thingService.watch(action.thingId, action.markedAsWatched);
  }
}
