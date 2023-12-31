import 'dart:async';

import '../../ethereum/errors/wallet_action_declined_error.dart';
import '../../general/contexts/multi_stage_operation_context.dart';
import '../../ethereum/services/user_operation_service.dart';
import '../../general/errors/handle_error.dart';
import '../../general/errors/insufficient_balance_error.dart';
import '../../general/utils/logger.dart';
import '../../user/errors/get_credential_error.dart';
import '../models/im/new_settlement_proposal_assessment_poll_vote_im.dart';
import '../../user/services/user_service.dart';
import '../../general/contracts/thing_validation_poll_contract.dart';
import '../../general/utils/utils.dart';
import '../models/im/decision_im.dart';
import '../../general/contracts/settlement_proposal_assessment_poll_contract.dart';
import '../models/vm/get_verifier_lottery_participants_rvm.dart';
import '../../general/contracts/settlement_proposal_assessment_verifier_lottery_contract.dart';
import '../../general/contracts/truquest_contract.dart';
import '../models/vm/get_settlement_proposal_rvm.dart';
import '../../general/contexts/document_context.dart';
import '../models/vm/get_votes_rvm.dart';
import 'settlement_api_service.dart';

class SettlementService {
  final SettlementApiService _settlementApiService;
  final UserService _userService;
  final UserOperationService _userOperationService;
  final TruQuestContract _truQuestContract;
  final ThingValidationPollContract _thingValidationPollContract;
  final SettlementProposalAssessmentVerifierLotteryContract _settlementProposalAssessmentVerifierLotteryContract;
  final SettlementProposalAssessmentPollContract _settlementProposalAssessmentPollContract;

  final _progress$Channel = StreamController<Stream<int>>();
  Stream<Stream<int>> get progress$$ => _progress$Channel.stream;

  SettlementService(
    this._settlementApiService,
    this._userService,
    this._userOperationService,
    this._truQuestContract,
    this._thingValidationPollContract,
    this._settlementProposalAssessmentVerifierLotteryContract,
    this._settlementProposalAssessmentPollContract,
  );

  Future createNewSettlementProposalDraft(DocumentContext documentContext) async {
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

  Future<GetSettlementProposalRvm?> getSettlementProposal(String proposalId) async {
    try {
      var result = await _settlementApiService.getSettlementProposal(proposalId);
      logger.info('ProposalId: ${result.proposal.id}');
      return result;
    } on HandleError {
      return null;
    }
  }

  Future<bool> checkThingAlreadyHasSettlementProposalUnderAssessment(String thingId) =>
      _truQuestContract.checkThingAlreadyHasSettlementProposalUnderAssessment(
        thingId,
      );

  Future submitNewSettlementProposal(String proposalId) async {
    await _settlementApiService.submitNewSettlementProposal(proposalId);
  }

  Stream<Object> fundSettlementProposal(
    String thingId,
    String proposalId,
    String signature,
    MultiStageOperationContext ctx,
  ) async* {
    logger.info('**************** Fund Proposal ****************');

    BigInt settlementProposalStake = await _truQuestContract.getSettlementProposalStake();
    BigInt availableFunds = await _userService.getAvailableFundsForCurrentUser();
    if (availableFunds < settlementProposalStake) {
      yield const InsufficientBalanceError();
      return;
    }

    yield _userOperationService.prepareOneWithRealTimeFeeUpdates(
      actions: [
        (
          _truQuestContract,
          _truQuestContract.fundSettlementProposal(
            thingId,
            proposalId,
            signature,
          )
        ),
      ],
      functionSignature: 'TruQuest.fundProposal(proposalId: $proposalId)',
      description: 'Fund the proposal to kick-start an evaluation process.',
      stakeSize: settlementProposalStake,
    );

    var userOp = await ctx.approveUserOpTask.future;
    if (userOp == null) return;

    var error = await _userOperationService.send(userOp);
    if (error != null) yield error;
  }

  Future<(String?, int?, int, int, bool?, bool?)> getVerifierLotteryInfo(
    String thingId,
    String proposalId,
  ) async {
    var currentUserId = _userService.latestCurrentUser?.id;
    var currentWalletAddress = _userService.latestCurrentUser?.walletAddress;

    int? initBlock = await _settlementProposalAssessmentVerifierLotteryContract.getLotteryInitBlock(
      thingId,
      proposalId,
    );
    int durationBlocks = await _settlementProposalAssessmentVerifierLotteryContract.getLotteryDurationBlocks();

    int thingVerifiersArrayIndex = currentWalletAddress != null
        ? await _thingValidationPollContract.getUserIndexAmongThingVerifiers(
            thingId,
            currentWalletAddress,
          )
        : -1;

    bool? alreadyClaimedASpot = currentWalletAddress != null
        ? await _settlementProposalAssessmentVerifierLotteryContract.checkAlreadyClaimedLotterySpot(
            thingId,
            proposalId,
            currentWalletAddress,
          )
        : null;

    bool? alreadyJoined = currentWalletAddress != null
        ? await _settlementProposalAssessmentVerifierLotteryContract.checkAlreadyJoinedLottery(
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
    int thingVerifiersArrayIndex,
    MultiStageOperationContext ctx,
  ) async* {
    logger.info('**************** Claim Lottery Spot ****************');

    BigInt verifierStake = await _truQuestContract.getVerifierStake();
    BigInt availableFunds = await _userService.getAvailableFundsForCurrentUser();
    if (availableFunds < verifierStake) {
      yield const InsufficientBalanceError();
      return;
    }

    yield _userOperationService.prepareOneWithRealTimeFeeUpdates(
      actions: [
        (
          _settlementProposalAssessmentVerifierLotteryContract,
          _settlementProposalAssessmentVerifierLotteryContract.claimLotterySpot(
            thingId,
            proposalId,
            thingVerifiersArrayIndex,
          )
        ),
      ],
      functionSignature: 'ProposalAssessmentVerifierLottery.claimASpot(proposalId: $proposalId)',
      description: 'Claim a verifier selection lottery spot.',
      stakeSize: verifierStake,
    );

    var userOp = await ctx.approveUserOpTask.future;
    if (userOp == null) return;

    var error = await _userOperationService.send(userOp);
    if (error != null) yield error;
  }

  Stream<Object> joinLottery(
    String thingId,
    String proposalId,
    MultiStageOperationContext ctx,
  ) async* {
    logger.info('**************** Join Lottery ****************');

    BigInt verifierStake = await _truQuestContract.getVerifierStake();
    BigInt availableFunds = await _userService.getAvailableFundsForCurrentUser();
    if (availableFunds < verifierStake) {
      yield const InsufficientBalanceError();
      return;
    }

    yield _userOperationService.prepareOneWithRealTimeFeeUpdates(
      actions: [
        (
          _settlementProposalAssessmentVerifierLotteryContract,
          _settlementProposalAssessmentVerifierLotteryContract.joinLottery(
            thingId,
            proposalId,
          )
        ),
      ],
      functionSignature: 'ProposalAssessmentVerifierLottery.join(proposalId: $proposalId)',
      description: 'Join the verifier selection lottery.',
      stakeSize: verifierStake,
    );

    var userOp = await ctx.approveUserOpTask.future;
    if (userOp == null) return;

    var error = await _userOperationService.send(userOp);
    if (error != null) yield error;
  }

  Future<GetVerifierLotteryParticipantsRvm> getVerifierLotteryParticipants(
    String thingId,
    String proposalId,
  ) async {
    var result = await _settlementApiService.getVerifierLotteryParticipants(
      thingId,
      proposalId,
    );
    return result;
  }

  Future<(String?, int?, int, int)> getAssessmentPollInfo(String thingId, String proposalId) async {
    var currentUserId = _userService.latestCurrentUser?.id;
    var currentWalletAddress = _userService.latestCurrentUser?.walletAddress;

    int? initBlock = await _settlementProposalAssessmentPollContract.getPollInitBlock(
      thingId,
      proposalId,
    );

    int durationBlocks = await _settlementProposalAssessmentPollContract.getPollDurationBlocks();

    int settlementProposalVerifiersArrayIndex = currentWalletAddress != null
        ? await _settlementProposalAssessmentPollContract.getUserIndexAmongSettlementProposalVerifiers(
            thingId,
            proposalId,
            currentWalletAddress,
          )
        : -1;

    return (
      currentUserId,
      initBlock,
      durationBlocks,
      settlementProposalVerifiersArrayIndex,
    );
  }

  Stream<Object> castVoteOffChain(
    String thingId,
    String proposalId,
    DecisionIm decision,
    String reason,
    MultiStageOperationContext ctx,
  ) async* {
    logger.info('**************** Cast Vote Off-chain ****************');

    var vote = NewSettlementProposalAssessmentPollVoteIm(
      thingId: thingId,
      proposalId: proposalId,
      castedAt: DateTime.now().getString(),
      decision: decision,
      reason: reason,
    );

    String signature;
    try {
      signature = await _userService.personalSign(vote.toMessageForSigning());
    } on WalletActionDeclinedError catch (error) {
      yield error;
      return;
    } on GetCredentialError catch (error) {
      yield error;
      return;
    }

    var ipfsCid = await _settlementApiService.castSettlementProposalAssessmentPollVote(vote, signature);

    logger.info('**************** Vote cid: $ipfsCid ****************');
  }

  Stream<Object> castVoteOnChain(
    String thingId,
    String proposalId,
    int settlementProposalVerifiersArrayIndex,
    DecisionIm decision,
    String reason,
    MultiStageOperationContext ctx,
  ) async* {
    logger.info('**************** Cast Vote On-chain ****************');

    yield _userOperationService.prepareOneWithRealTimeFeeUpdates(
      actions: [
        (
          _settlementProposalAssessmentPollContract,
          _settlementProposalAssessmentPollContract.castVote(
            thingId,
            proposalId,
            settlementProposalVerifiersArrayIndex,
            decision,
            reason,
          )
        ),
      ],
      functionSignature:
          'ProposalAssessmentPoll.castVote(proposalId: $proposalId, decision: "${decision.getString()}")',
      description: 'Cast a vote indicating your decision regarding the proposal.',
    );

    var userOp = await ctx.approveUserOpTask.future;
    if (userOp == null) return;

    var error = await _userOperationService.send(userOp);
    if (error != null) yield error;
  }

  Future<GetVotesRvm> getVotes(String proposalId) async {
    var result = await _settlementApiService.getVotes(proposalId);
    return result;
  }
}
