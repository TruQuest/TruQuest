import 'dart:async';

import 'package:rxdart/rxdart.dart';

import '../../general/contexts/multi_stage_operation_context.dart';
import '../models/vm/validation_poll_info_vm.dart';
import '../models/vm/get_verifier_lottery_participants_rvm.dart';
import '../models/vm/get_votes_rvm.dart';
import '../models/vm/settlement_proposal_preview_vm.dart';
import '../models/vm/thing_state_vm.dart';
import '../models/vm/get_thing_rvm.dart';
import '../models/vm/verifier_lottery_info_vm.dart';
import '../../general/bloc/bloc.dart';
import '../services/thing_service.dart';
import 'thing_actions.dart';

class ThingBloc extends Bloc<ThingAction> {
  final ThingService _thingService;

  final _thingChannel = StreamController<GetThingRvm?>.broadcast();
  Stream<GetThingRvm?> get thing$ => _thingChannel.stream;

  final _verifierLotteryInfoChannel = StreamController<VerifierLotteryInfoVm>.broadcast();
  Stream<VerifierLotteryInfoVm> get verifierLotteryInfo$ => _verifierLotteryInfoChannel.stream;

  final _verifierLotteryParticipantsChannel = BehaviorSubject<GetVerifierLotteryParticipantsRvm>();
  Stream<GetVerifierLotteryParticipantsRvm> get verifierLotteryParticipants$ =>
      _verifierLotteryParticipantsChannel.stream;

  final _votesChannel = BehaviorSubject<GetVotesRvm>();
  Stream<GetVotesRvm> get votes$ => _votesChannel.stream;

  final _proposalsListChannel = BehaviorSubject<List<SettlementProposalPreviewVm>>();
  Stream<List<SettlementProposalPreviewVm>> get proposalsList$ => _proposalsListChannel.stream;

  ThingBloc(super.toastMessenger, this._thingService) {
    onAction = (action) async {
      if (action is GetThing) {
        await _getThing(action);
      } else if (action is GetVerifierLotteryParticipants) {
        await _getVerifierLotteryParticipants(action);
      } else if (action is GetVotes) {
        await _getVotes(action);
      } else if (action is GetSettlementProposalsList) {
        await _getSettlementProposalsList(action);
      } else if (action is Watch) {
        await _watch(action);
      }
    };
  }

  @override
  Future<Object?> handleExecute(ThingAction action) {
    if (action is CreateNewThingDraft) {
      return _createNewThingDraft(action);
    } else if (action is SubmitNewThing) {
      return _submitNewThing(action);
    } else if (action is GetVerifierLotteryInfo) {
      return _getVerifierLotteryInfo(action);
    } else if (action is GetValidationPollInfo) {
      return _getValidationPollInfo(action);
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

  Future _getThing(GetThing action) async {
    var result = await _thingService.getThing(action.thingId);
    if (result == null) {
      _thingChannel.add(null);
      return;
    }

    if (result.thing.state == ThingStateVm.awaitingFunding) {
      bool thingFunded = await _thingService.checkThingAlreadyFunded(action.thingId);
      result = GetThingRvm(
        thing: result.thing.copyWith(fundedAwaitingConfirmation: thingFunded),
        signature: result.signature,
      );
    }

    _thingChannel.add(result);
  }

  Future<bool> _submitNewThing(SubmitNewThing action) async {
    await _thingService.submitNewThing(action.thingId);
    _getThing(GetThing(thingId: action.thingId));
    return true;
  }

  Stream<Object> _fundThing(FundThing action, MultiStageOperationContext ctx) =>
      _thingService.fundThing(action.thingId, action.signature, ctx);

  Future<VerifierLotteryInfoVm> _getVerifierLotteryInfo(GetVerifierLotteryInfo action) async {
    var result = await _thingService.getVerifierLotteryInfo(action.thingId);
    var info = VerifierLotteryInfoVm(
      userId: result.$1,
      initBlock: result.$2,
      durationBlocks: result.$3,
      alreadyJoined: result.$4,
    );
    _verifierLotteryInfoChannel.add(info);

    return info;
  }

  Stream<Object> _joinLottery(JoinLottery action, MultiStageOperationContext ctx) =>
      _thingService.joinLottery(action.thingId, ctx);

  Future _getVerifierLotteryParticipants(GetVerifierLotteryParticipants action) async {
    var result = await _thingService.getVerifierLotteryParticipants(action.thingId);
    _verifierLotteryParticipantsChannel.add(result);
  }

  Future<ValidationPollInfoVm> _getValidationPollInfo(GetValidationPollInfo action) async {
    var info = await _thingService.getValidationPollInfo(action.thingId);
    return ValidationPollInfoVm(
      userId: info.$1,
      initBlock: info.$2,
      durationBlocks: info.$3,
      thingVerifiersArrayIndex: info.$4,
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
        action.thingVerifiersArrayIndex,
        action.decision,
        action.reason,
        ctx,
      );

  Future _getVotes(GetVotes action) async {
    var result = await _thingService.getVotes(action.thingId);
    _votesChannel.add(result);
  }

  Future _getSettlementProposalsList(GetSettlementProposalsList action) async {
    var result = await _thingService.getSettlementProposalsList(action.thingId);
    _proposalsListChannel.add(result.proposals);
  }

  Future _watch(Watch action) async {
    await _thingService.watch(action.thingId, action.markedAsWatched);
  }
}
