import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

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
    return Theme(
      data: ThemeData(
        brightness: Brightness.dark,
        colorScheme: Theme.of(context).colorScheme.copyWith(
              brightness: Brightness.dark,
              secondary: Color(0xffF8F9FA),
            ),
      ),
      child: Column(
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

                var failure = await action.result;
                return failure == null;
              },
            ),
            steps: [
              Step(
                title: Text(
                  'Claim a spot',
                  style: GoogleFonts.philosopher(
                    color: Color(0xffF8F9FA),
                    fontSize: 16,
                  ),
                ),
                content: Padding(
                  padding: const EdgeInsets.only(bottom: 12),
                  child: Text(
                    'Thing verifiers can claim a spot instead of going through the lottery',
                    style: GoogleFonts.raleway(
                      color: Colors.white,
                    ),
                  ),
                ),
                isActive: true,
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

                  var failure = await action.result;
                  if (failure == null) {
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

                var failure = await action.result;
                return failure == null;
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
                title: Text(
                  'Commit to lottery',
                  style: GoogleFonts.philosopher(
                    color: Color(0xffF8F9FA),
                    fontSize: 16,
                  ),
                ),
                content: Padding(
                  padding: const EdgeInsets.only(bottom: 12),
                  child: Text(
                    'Committing to lottery means staking some amount of Truthserum for the duration of the lottery',
                    style: GoogleFonts.raleway(
                      color: Colors.white,
                    ),
                  ),
                ),
                isActive: true,
              ),
              Step(
                title: Text(
                  'Join lottery',
                  style: GoogleFonts.philosopher(
                    color: Color(0xffF8F9FA),
                    fontSize: 16,
                  ),
                ),
                content: Padding(
                  padding: const EdgeInsets.only(bottom: 12),
                  child: Text(
                    'Joining lottery generates a one-time random number (nonce) which will be used in the lottery process to determine winners',
                    style: GoogleFonts.raleway(
                      color: Colors.white,
                    ),
                  ),
                ),
                isActive: true,
              ),
            ],
          ),
        ],
      ),
    );
  }
}
