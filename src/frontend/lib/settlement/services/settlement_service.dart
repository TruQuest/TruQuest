import 'dart:async';

import '../../ethereum/errors/wallet_action_declined_error.dart';
import '../../general/contexts/multi_stage_operation_context.dart';
import '../../ethereum/services/user_operation_service.dart';
import '../../user/errors/wallet_locked_error.dart';
import '../models/im/new_assessment_poll_vote_im.dart';
import '../../user/services/user_service.dart';
import '../../general/contracts/acceptance_poll_contract.dart';
import '../../general/extensions/datetime_extension.dart';
import '../../general/models/rvm/verifier_lottery_participant_entry_vm.dart';
import '../models/im/decision_im.dart';
import '../../general/contracts/assessment_poll_contract.dart';
import '../models/rvm/get_verifier_lottery_participants_rvm.dart';
import '../../general/contracts/thing_assessment_verifier_lottery_contract.dart';
import '../../general/contracts/truquest_contract.dart';
import '../models/rvm/get_settlement_proposal_rvm.dart';
import '../../general/contexts/document_context.dart';
import '../models/rvm/get_verifiers_rvm.dart';
import 'settlement_api_service.dart';

class SettlementService {
  final SettlementApiService _settlementApiService;
  final UserService _userService;
  final UserOperationService _userOperationService;
  final TruQuestContract _truQuestContract;
  final AcceptancePollContract _acceptancePollContract;
  final ThingAssessmentVerifierLotteryContract _thingAssessmentVerifierLotteryContract;
  final AssessmentPollContract _assessmentPollContract;

  final _progress$Channel = StreamController<Stream<int>>();
  Stream<Stream<int>> get progress$$ => _progress$Channel.stream;

  SettlementService(
    this._settlementApiService,
    this._userService,
    this._userOperationService,
    this._truQuestContract,
    this._acceptancePollContract,
    this._thingAssessmentVerifierLotteryContract,
    this._assessmentPollContract,
  );

  Future createNewSettlementProposalDraft(
    DocumentContext documentContext,
  ) async {
    var progress$ = await _settlementApiService.createNewSettlementProposalDraft(
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
  ) =>
      _truQuestContract.checkThingAlreadyHasSettlementProposalUnderAssessment(
        thingId,
      );

  Future submitNewSettlementProposal(String proposalId) async {
    await _settlementApiService.submitNewSettlementProposal(
      proposalId,
    );
  }

  Stream<Object> fundSettlementProposal(
    String thingId,
    String proposalId,
    String signature,
    MultiStageOperationContext ctx,
  ) async* {
    print('**************** Fund Proposal ****************');

    if (!_userService.walletUnlocked) {
      yield const WalletLockedError();

      bool unlocked = await ctx.unlockWalletTask.future;
      if (!unlocked) {
        return;
      }
    }

    // @@TODO!!: Check balance!
    // int balance = await _truthserumContract.balanceOf(
    //   _walletService.currentWalletAddress!,
    // );
    // print('**************** Balance: $balance drops ****************');
    // if (balance < amount) {
    //   yield const InsufficientBalanceError();
    //   return;
    // }

    yield _userOperationService.prepareOneWithRealTimeFeeUpdates(
      actions: [
        (
          TruQuestContract.address,
          _truQuestContract.fundThingSettlementProposal(
            thingId,
            proposalId,
            signature,
          )
        ),
      ],
    );

    var userOp = await ctx.approveUserOpTask.future;
    if (userOp == null) {
      return;
    }

    var error = await _userOperationService.sendUserOp(userOp);
    if (error != null) {
      yield error;
    }
  }

  Future<(String?, int?, int, int, bool?, bool?)> getVerifierLotteryInfo(
    String thingId,
    String proposalId,
  ) async {
    var currentUserId = _userService.latestCurrentUser?.id;
    var currentWalletAddress = _userService.latestCurrentUser?.walletAddress;

    int? initBlock = await _thingAssessmentVerifierLotteryContract.getLotteryInitBlock(thingId, proposalId);

    int durationBlocks = await _thingAssessmentVerifierLotteryContract.getLotteryDurationBlocks();

    int thingVerifiersArrayIndex = currentWalletAddress != null
        ? await _acceptancePollContract.getUserIndexAmongThingVerifiers(
            thingId,
            currentWalletAddress,
          )
        : -1;

    bool? alreadyClaimedASpot = currentWalletAddress != null
        ? await _thingAssessmentVerifierLotteryContract.checkAlreadyClaimedLotterySpot(
            thingId,
            proposalId,
            currentWalletAddress,
          )
        : null;

    bool? alreadyJoined = currentWalletAddress != null
        ? await _thingAssessmentVerifierLotteryContract.checkAlreadyJoinedLottery(
            thingId,
            proposalId,
            currentWalletAddress,
          )
        : null;

    return (
      currentUserId,
      initBlock,
      durationBlocks,
      thingVerifiersArrayIndex,
      alreadyClaimedASpot,
      alreadyJoined,
    );
  }

  Stream<Object> claimLotterySpot(
    String thingId,
    String proposalId,
    int userIndexInThingVerifiersArray,
    MultiStageOperationContext ctx,
  ) async* {
    print('**************** Claim Lottery Spot ****************');

    if (!_userService.walletUnlocked) {
      yield const WalletLockedError();

      bool unlocked = await ctx.unlockWalletTask.future;
      if (!unlocked) {
        return;
      }
    }

    // @@TODO!!: Check balance!
    // int balance = await _truthserumContract.balanceOf(
    //   _walletService.currentWalletAddress!,
    // );
    // print('**************** Balance: $balance drops ****************');
    // if (balance < amount) {
    //   yield const InsufficientBalanceError();
    //   return;
    // }

    yield _userOperationService.prepareOneWithRealTimeFeeUpdates(
      actions: [
        (
          ThingAssessmentVerifierLotteryContract.address,
          _thingAssessmentVerifierLotteryContract.claimLotterySpot(
            thingId,
            proposalId,
            userIndexInThingVerifiersArray,
          )
        ),
      ],
    );

    var userOp = await ctx.approveUserOpTask.future;
    if (userOp == null) {
      return;
    }

    var error = await _userOperationService.sendUserOp(userOp);
    if (error != null) {
      yield error;
    }
  }

  Stream<Object> joinLottery(
    String thingId,
    String proposalId,
    MultiStageOperationContext ctx,
  ) async* {
    print('**************** Join Lottery ****************');

    if (!_userService.walletUnlocked) {
      yield const WalletLockedError();

      bool unlocked = await ctx.unlockWalletTask.future;
      if (!unlocked) {
        return;
      }
    }

    // @@TODO!!: Check balance!
    // int balance = await _truthserumContract.balanceOf(
    //   _walletService.currentWalletAddress!,
    // );
    // print('**************** Balance: $balance drops ****************');
    // if (balance < amount) {
    //   yield const InsufficientBalanceError();
    //   return;
    // }

    yield _userOperationService.prepareOneWithRealTimeFeeUpdates(
      actions: [
        (
          ThingAssessmentVerifierLotteryContract.address,
          _thingAssessmentVerifierLotteryContract.joinLottery(
            thingId,
            proposalId,
          )
        ),
      ],
    );

    var userOp = await ctx.approveUserOpTask.future;
    if (userOp == null) {
      return;
    }

    var error = await _userOperationService.sendUserOp(userOp);
    if (error != null) {
      yield error;
    }
  }

  Future<GetVerifierLotteryParticipantsRvm> getVerifierLotteryParticipants(
    String thingId,
    String proposalId,
  ) async {
    var result = await _settlementApiService.getVerifierLotteryParticipants(
      proposalId,
    );

    var (lotteryInitBlock, dataHash, userXorDataHash) =
        await _thingAssessmentVerifierLotteryContract.getOrchestratorCommitment(thingId, proposalId);

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

  Future<(String?, int?, int, int)> getAssessmentPollInfo(
    String thingId,
    String proposalId,
  ) async {
    var currentUserId = _userService.latestCurrentUser?.id;
    var currentWalletAddress = _userService.latestCurrentUser?.walletAddress;

    int? initBlock = await _assessmentPollContract.getPollInitBlock(
      thingId,
      proposalId,
    );

    int durationBlocks = await _assessmentPollContract.getPollDurationBlocks();

    int proposalVerifiersArrayIndex = currentWalletAddress != null
        ? await _assessmentPollContract.getUserIndexAmongProposalVerifiers(
            thingId,
            proposalId,
            currentWalletAddress,
          )
        : -1;

    return (
      currentUserId,
      initBlock,
      durationBlocks,
      proposalVerifiersArrayIndex,
    );
  }

  Stream<Object> castVoteOffChain(
    String thingId,
    String proposalId,
    DecisionIm decision,
    String reason,
    MultiStageOperationContext ctx,
  ) async* {
    print('**************** Cast Vote Off-chain ****************');

    var vote = NewAssessmentPollVoteIm(
      thingId: thingId,
      proposalId: proposalId,
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

    var ipfsCid = await _settlementApiService.castThingSettlementProposalAssessmentPollVote(vote, signature);

    print('**************** Vote cid: $ipfsCid ****************');
  }

  Stream<Object> castVoteOnChain(
    String thingId,
    String proposalId,
    int userIndexInProposalVerifiersArray,
    DecisionIm decision,
    String reason,
    MultiStageOperationContext ctx,
  ) async* {
    print('**************** Cast Vote On-chain ****************');

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
          AssessmentPollContract.address,
          _assessmentPollContract.castVote(
            thingId,
            proposalId,
            userIndexInProposalVerifiersArray,
            decision,
            reason,
          )
        ),
      ],
    );

    var userOp = await ctx.approveUserOpTask.future;
    if (userOp == null) {
      return;
    }

    var error = await _userOperationService.sendUserOp(userOp);
    if (error != null) {
      yield error;
    }
  }

  Future<GetVerifiersRvm> getVerifiers(String proposalId) async {
    var result = await _settlementApiService.getVerifiers(proposalId);
    return result;
  }
}
