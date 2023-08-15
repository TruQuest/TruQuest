import 'dart:async';

import 'package:either_dart/either.dart';

import '../../ethereum/errors/wallet_action_declined_error.dart';
import '../../general/contexts/multi_stage_operation_context.dart';
import '../../general/errors/insufficient_balance_error.dart';
import '../../user/errors/wallet_locked_error.dart';
import '../../user/services/user_service.dart';
import '../../ethereum/services/user_operation_service.dart';
import '../models/im/new_acceptance_poll_vote_im.dart';
import '../../general/models/rvm/verifier_lottery_participant_entry_vm.dart';
import '../errors/thing_error.dart';
import '../models/rvm/get_settlement_proposals_list_rvm.dart';
import '../../general/utils/utils.dart';
import '../../general/contracts/acceptance_poll_contract.dart';
import '../../general/contracts/thing_submission_verifier_lottery_contract.dart';
import '../models/im/decision_im.dart';
import '../models/rvm/get_thing_rvm.dart';
import '../../general/contracts/truquest_contract.dart';
import '../../general/contexts/document_context.dart';
import '../models/rvm/get_verifier_lottery_participants_rvm.dart';
import '../models/rvm/get_votes_rvm.dart';
import '../models/rvm/submit_new_thing_rvm.dart';
import 'thing_api_service.dart';

class ThingService {
  final ThingApiService _thingApiService;
  final UserService _userService;
  final UserOperationService _userOperationService;
  final TruQuestContract _truQuestContract;
  final ThingSubmissionVerifierLotteryContract _thingSubmissionVerifierLotteryContract;
  final AcceptancePollContract _acceptancePollContract;

  final _progress$Channel = StreamController<Stream<int>>();
  Stream<Stream<int>> get progress$$ => _progress$Channel.stream;

  ThingService(
    this._thingApiService,
    this._userService,
    this._userOperationService,
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
      documentContext.tags.toList(),
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

  Future<bool> checkThingAlreadyFunded(String thingId) => _truQuestContract.checkThingAlreadyFunded(thingId);

  Future<SubmitNewThingRvm> submitNewThing(String thingId) async {
    var result = await _thingApiService.submitNewThing(thingId);
    return result;
  }

  Stream<Object> fundThing(
    String thingId,
    String signature,
    MultiStageOperationContext ctx,
  ) async* {
    print('**************** Fund thing ****************');

    if (!_userService.walletUnlocked) {
      yield const WalletLockedError();

      bool unlocked = await ctx.unlockWalletTask.future;
      if (!unlocked) {
        return;
      }
    }

    BigInt thingSubmissionStake = await _truQuestContract.getThingSubmissionStake();
    BigInt availableFunds = await _userService.getAvailableFundsForCurrentUser();
    if (availableFunds < thingSubmissionStake) {
      yield const InsufficientBalanceError();
      return;
    }

    yield _userOperationService.prepareOneWithRealTimeFeeUpdates(
      actions: [
        (TruQuestContract.address, _truQuestContract.fundThing(thingId, signature)),
      ],
      functionSignature: 'TruQuest.fundPromise(promiseId: $thingId)',
      description: 'Fund the promise to kick-start an evaluation process.',
      stakeSize: thingSubmissionStake,
    );

    var userOp = await ctx.approveUserOpTask.future;
    if (userOp == null) {
      return;
    }

    var error = await _userOperationService.send(userOp);
    if (error != null) {
      yield error;
    }
  }

  Future<(String?, int?, int, bool?)> getVerifierLotteryInfo(
    String thingId,
  ) async {
    var currentUserId = _userService.latestCurrentUser?.id;
    var currentWalletAddress = _userService.latestCurrentUser?.walletAddress;

    int? initBlock = await _thingSubmissionVerifierLotteryContract.getLotteryInitBlock(thingId);

    int durationBlocks = await _thingSubmissionVerifierLotteryContract.getLotteryDurationBlocks();

    // @@NOTE: If user has a wallet but is not signed-in, then checking is
    // kind of pointless since the join button would be hidden anyway, but whatever.
    bool? alreadyJoined = currentWalletAddress != null
        ? await _thingSubmissionVerifierLotteryContract.checkAlreadyJoinedLottery(
            thingId,
            currentWalletAddress,
          )
        : null;

    return (
      currentUserId,
      initBlock,
      durationBlocks,
      alreadyJoined,
    );
  }

  Stream<Object> joinLottery(
    String thingId,
    MultiStageOperationContext ctx,
  ) async* {
    // @@??: Need to refresh the info after joining because otherwise imagine this:
    // User joins and stays on the lottery page waiting for it to complete.
    // Once it does the swipe button's value key, which has the form
    // '${info.userId}::${currentBlock < endBlock}::${info.alreadyJoined}',
    // changes in the middle bit but, since the info hasn't been refreshed after joining,
    // the 'swiped' property is still 'false', which results in the button being
    // disabled but unswiped, whereas it would be more logical for it to be swiped.

    print('******************** Join Lottery ********************');

    if (!_userService.walletUnlocked) {
      yield const WalletLockedError();

      bool unlocked = await ctx.unlockWalletTask.future;
      if (!unlocked) {
        return;
      }
    }

    BigInt verifierStake = await _truQuestContract.getVerifierStake();
    BigInt availableFunds = await _userService.getAvailableFundsForCurrentUser();
    if (availableFunds < verifierStake) {
      yield const InsufficientBalanceError();
      return;
    }

    yield _userOperationService.prepareOneWithRealTimeFeeUpdates(
      actions: [
        (ThingSubmissionVerifierLotteryContract.address, _thingSubmissionVerifierLotteryContract.joinLottery(thingId)),
      ],
      functionSignature: 'PromiseSubmissionVerifierLotery.join(promiseId: $thingId)',
      description: 'Join the verifier selection lottery.',
      stakeSize: verifierStake,
    );

    var userOp = await ctx.approveUserOpTask.future;
    if (userOp == null) {
      return;
    }

    var error = await _userOperationService.send(userOp);
    if (error != null) {
      yield error;
    }
  }

  Future<GetVerifierLotteryParticipantsRvm> getVerifierLotteryParticipants(
    String thingId,
  ) async {
    var result = await _thingApiService.getVerifierLotteryParticipants(thingId);

    var (lotteryInitBlock, dataHash, userXorDataHash) =
        await _thingSubmissionVerifierLotteryContract.getOrchestratorCommitment(thingId);

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

  Future<(String?, int?, int, int)> getAcceptancePollInfo(
    String thingId,
  ) async {
    var currentUserId = _userService.latestCurrentUser?.id;
    var currentWalletAddress = _userService.latestCurrentUser?.walletAddress;

    int? initBlock = await _acceptancePollContract.getPollInitBlock(thingId);
    int durationBlocks = await _acceptancePollContract.getPollDurationBlocks();

    int thingVerifiersArrayIndex = currentWalletAddress != null
        ? await _acceptancePollContract.getUserIndexAmongThingVerifiers(
            thingId,
            currentWalletAddress,
          )
        : -1;

    return (
      currentUserId,
      initBlock,
      durationBlocks,
      thingVerifiersArrayIndex,
    );
  }

  Stream<Object> castVoteOffChain(
    String thingId,
    DecisionIm decision,
    String reason,
    MultiStageOperationContext ctx,
  ) async* {
    print('********************** Cast Vote Off-chain **********************');

    var vote = NewAcceptancePollVoteIm(
      thingId: thingId,
      castedAt: DateTime.now().getString(),
      decision: decision,
      reason: reason,
    );

    if (!_userService.walletUnlocked) {
      yield const WalletLockedError();

      bool unlocked = await ctx.unlockWalletTask.future;
      if (!unlocked) {
        return;
      }
    }

    String signature;
    try {
      signature = await _userService.personalSign(
        vote.toMessageForSigning(),
      );
    } on WalletActionDeclinedError catch (error) {
      print(error.message);
      yield error;
      return;
    }

    var ipfsCid = await _thingApiService.castThingAcceptancePollVote(
      vote,
      signature,
    );

    print('**************** Vote cid: $ipfsCid ****************');
  }

  Stream<Object> castVoteOnChain(
    String thingId,
    int userIndexInThingVerifiersArray,
    DecisionIm decision,
    String reason,
    MultiStageOperationContext ctx,
  ) async* {
    print('********************** Cast Vote On-chain **********************');

    if (!_userService.walletUnlocked) {
      yield const WalletLockedError();

      bool unlocked = await ctx.unlockWalletTask.future;
      if (!unlocked) {
        return;
      }
    }

    yield _userOperationService.prepareOneWithRealTimeFeeUpdates(
      actions: [
        (
          AcceptancePollContract.address,
          _acceptancePollContract.castVote(
            thingId,
            userIndexInThingVerifiersArray,
            decision,
            reason,
          )
        ),
      ],
      functionSignature: 'PromiseAcceptancePoll.castVote(promiseId: $thingId, decision: "${decision.getString()}")',
      description: 'Cast a vote indicating your decision regarding the promise.',
    );

    var userOp = await ctx.approveUserOpTask.future;
    if (userOp == null) {
      return;
    }

    var error = await _userOperationService.send(userOp);
    if (error != null) {
      yield error;
    }
  }

  Future<GetVotesRvm> getVotes(String thingId) async {
    var result = await _thingApiService.getVotes(thingId);
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
