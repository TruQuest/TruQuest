import 'dart:async';
import 'dart:convert';

import 'package:either_dart/either.dart';

import '../../user/services/user_service.dart';
import '../../ethereum/services/user_operation_service.dart';
import '../../ethereum/services/ethereum_rpc_provider.dart';
import '../../ethereum/services/local_wallet_service.dart';
import '../models/im/new_acceptance_poll_vote_im.dart';
import '../../user/errors/wallet_locked_error.dart';
import '../../general/models/rvm/verifier_lottery_participant_entry_vm.dart';
import '../errors/thing_error.dart';
import '../models/rvm/get_settlement_proposals_list_rvm.dart';
import '../models/rvm/get_verifiers_rvm.dart';
import '../../general/extensions/datetime_extension.dart';
import '../../general/contracts/acceptance_poll_contract.dart';
import '../../general/contracts/thing_submission_verifier_lottery_contract.dart';
import '../models/im/decision_im.dart';
import '../models/rvm/get_thing_rvm.dart';
import '../../general/contracts/truquest_contract.dart';
import '../../general/contexts/document_context.dart';
import '../models/rvm/get_verifier_lottery_participants_rvm.dart';
import '../models/rvm/submit_new_thing_rvm.dart';
import 'thing_api_service.dart';
import '../../general/errors/error.dart';

class ThingService {
  final ThingApiService _thingApiService;
  final LocalWalletService _localWalletService;
  final UserService _userService;
  final EthereumRpcProvider _ethereumRpcProvider;
  final UserOperationService _userOperationService;
  final TruQuestContract _truQuestContract;
  final ThingSubmissionVerifierLotteryContract
      _thingSubmissionVerifierLotteryContract;
  final AcceptancePollContract _acceptancePollContract;

  final StreamController<Stream<int>> _progress$Channel =
      StreamController<Stream<int>>();
  Stream<Stream<int>> get progress$$ => _progress$Channel.stream;

  ThingService(
    this._thingApiService,
    this._localWalletService,
    this._userService,
    this._ethereumRpcProvider,
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

  Future<bool> checkThingAlreadyFunded(String thingId) =>
      _truQuestContract.checkThingAlreadyFunded(thingId);

  Future<SubmitNewThingRvm> submitNewThing(String thingId) async {
    var result = await _thingApiService.submitNewThing(thingId);
    return result;
  }

  Future<Error?> fundThing(String thingId, String signature) async {
    // var wallet = _smartWalletService.wallet!;
    // if (wallet.locked) {
    //   return WalletLockedError();
    // }

    // print('**************** Fund thing ****************');

    // return await _userOperationService.send(
    //   from: wallet,
    //   target: TruQuestContract.address,
    //   action: _truQuestContract.fundThing(thingId, signature),
    // );
  }

  Future<(String?, int?, int, bool?)> getVerifierLotteryInfo(
    String thingId,
  ) async {
    var currentUserId = _userService.latestCurrentUser?.id;
    var currentWalletAddress = _userService.latestCurrentUser?.walletAddress;

    int? initBlock = await _thingSubmissionVerifierLotteryContract
        .getLotteryInitBlock(thingId);

    int durationBlocks = await _thingSubmissionVerifierLotteryContract
        .getLotteryDurationBlocks();

    // @@NOTE: If user has a wallet but is not signed-in, then checking is
    // kind of pointless since the join button would be hidden anyway, but whatever.
    bool? alreadyJoined = currentWalletAddress != null
        ? await _thingSubmissionVerifierLotteryContract
            .checkAlreadyJoinedLottery(
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

  Future<Error?> joinLottery(String thingId) async {
    // var wallet = _smartWalletService.wallet!;
    // if (wallet.locked) {
    //   return WalletLockedError();
    // }

    // print('******************** Join Lottery ********************');

    // return await _userOperationService.send(
    //   from: wallet,
    //   target: ThingSubmissionVerifierLotteryContract.address,
    //   action: _thingSubmissionVerifierLotteryContract.joinLottery(thingId),
    // );
  }

  Future<GetVerifierLotteryParticipantsRvm> getVerifierLotteryParticipants(
    String thingId,
  ) async {
    var result = await _thingApiService.getVerifierLotteryParticipants(thingId);

    var (lotteryInitBlock, dataHash, userXorDataHash) =
        await _thingSubmissionVerifierLotteryContract
            .getOrchestratorCommitment(thingId);

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

  Future<Error?> castVoteOffChain(
    String thingId,
    DecisionIm decision,
    String reason,
  ) async {
    // var wallet = _smartWalletService.wallet!;
    // if (wallet.locked) {
    //   return WalletLockedError();
    // }

    // var vote = NewAcceptancePollVoteIm(
    //   thingId: thingId,
    //   castedAt: DateTime.now().getString(),
    //   decision: decision,
    //   reason: reason,
    // );

    // var signature = wallet.ownerSign(jsonEncode(vote.toJsonForSigning()));

    // var ipfsCid = await _thingApiService.castThingAcceptancePollVote(
    //   vote,
    //   signature,
    // );

    // print('**************** Vote cid: $ipfsCid ****************');

    return null;
  }

  Future<Error?> castVoteOnChain(
    String thingId,
    int userIndexInThingVerifiersArray,
    DecisionIm decision,
    String reason,
  ) async {
    // var wallet = _smartWalletService.wallet!;
    // if (wallet.locked) {
    //   return WalletLockedError();
    // }

    // print('********************** Cast Vote **********************');

    // return await _userOperationService.send(
    //   from: wallet,
    //   target: AcceptancePollContract.address,
    //   action: _acceptancePollContract.castVote(
    //     thingId,
    //     userIndexInThingVerifiersArray,
    //     decision,
    //     reason,
    //   ),
    // );
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
