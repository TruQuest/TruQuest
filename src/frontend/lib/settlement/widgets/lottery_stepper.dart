import 'package:flutter/material.dart';

import '../models/rvm/settlement_proposal_vm.dart';
import '../../general/widgets/swipe_button.dart';
import '../../widget_extensions.dart';
import '../bloc/settlement_actions.dart';
import '../bloc/settlement_bloc.dart';
import '../bloc/settlement_result_vm.dart';

class LotteryStepper extends StatefulWidget {
  final SettlementProposalVm proposal;
  final GetVerifierLotteryInfoSuccessVm info;
  final int currentBlock;
  final int endBlock;

  const LotteryStepper({
    super.key,
    required this.proposal,
    required this.info,
    required this.currentBlock,
    required this.endBlock,
  });

  @override
  State<LotteryStepper> createState() => _LotteryStepperState();
}

class _LotteryStepperState extends StateX<LotteryStepper> {
  late final _settlementBloc = use<SettlementBloc>();

  int _currentStep = 0;

  bool _checkButtonShouldBeEnabled(int stepIndex) {
    var info = widget.info;
    if (stepIndex == -1) {
      // @@TODO: Is the thing's verifier.
      return info.initBlock != null &&
          info.alreadyPreJoined != null &&
          info.alreadyClaimedASpot != null &&
          !info.alreadyPreJoined! &&
          !info.alreadyClaimedASpot! &&
          widget.currentBlock < widget.endBlock;
    } else if (stepIndex == 0) {
      return info.initBlock != null &&
          info.alreadyPreJoined != null &&
          !info.alreadyPreJoined! &&
          widget.currentBlock < widget.endBlock - 1;
    }

    return info.initBlock != null &&
        info.alreadyPreJoined != null &&
        info.alreadyJoined != null &&
        info.alreadyPreJoined! &&
        !info.alreadyJoined! &&
        widget.currentBlock < widget.endBlock;
  }

  bool _checkButtonShouldBeSwiped(int stepIndex) {
    var info = widget.info;
    if (stepIndex == -1) {
      return info.alreadyClaimedASpot != null && info.alreadyClaimedASpot!;
    } else if (stepIndex == 0) {
      return info.alreadyPreJoined != null &&
          info.alreadyClaimedASpot != null &&
          info.alreadyPreJoined! &&
          !info.alreadyClaimedASpot!;
    }

    return info.alreadyJoined != null &&
        info.alreadyClaimedASpot != null &&
        info.alreadyJoined! &&
        !info.alreadyClaimedASpot!;
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Stepper(
          controlsBuilder: (context, details) => SwipeButton(
            text: 'Slide to claim',
            enabled: _checkButtonShouldBeEnabled(-1),
            swiped: _checkButtonShouldBeSwiped(-1),
            onCompletedSwipe: () async {
              var action = ClaimLotterySpot(
                thingId: widget.proposal.thingId,
                proposalId: widget.proposal.id,
              );
              _settlementBloc.dispatch(action);

              var error = await action.result;
              return error == null;
            },
          ),
          steps: [
            Step(
              title: Text('Claim a spot'),
              content: Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: Text(
                    'Thing verifiers can claim a spot instead of going through the lottery'),
              ),
            ),
          ],
        ),
        SizedBox(height: 6),
        Divider(
          color: Colors.white70,
          indent: 116,
          endIndent: 80,
        ),
        SizedBox(height: 6),
        Stepper(
          currentStep: _currentStep,
          controlsBuilder: (context, details) => SwipeButton(
            text: 'Slide to ${details.currentStep == 0 ? 'commit' : 'join'}',
            enabled: _checkButtonShouldBeEnabled(details.currentStep),
            swiped: _checkButtonShouldBeSwiped(details.currentStep),
            onCompletedSwipe: () async {
              if (details.currentStep == 0) {
                var action = PreJoinLottery(
                  thingId: widget.proposal.thingId,
                  proposalId: widget.proposal.id,
                );
                _settlementBloc.dispatch(action);

                var error = await action.result;
                if (error == null) {
                  details.onStepContinue!();
                  return true;
                }

                return false;
              }

              var action = JoinLottery(
                thingId: widget.proposal.thingId,
                proposalId: widget.proposal.id,
              );
              _settlementBloc.dispatch(action);

              var error = await action.result;
              return error == null;
            },
          ),
          onStepContinue: () => setState(() {
            _currentStep++;
          }),
          onStepTapped: (value) => setState(() {
            _currentStep = value;
          }),
          steps: [
            Step(
              title: Text('Commit to lottery'),
              content: Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: Text(
                    'Committing to lottery means staking some amount of Truthserum for the duration of the lottery'),
              ),
            ),
            Step(
              title: Text('Join lottery'),
              content: Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: Text(
                    'Joining lottery generates a one-time random number (nonce) which will be used in the lottery process to determine winners'),
              ),
            ),
          ],
        ),
      ],
    );
  }
}
