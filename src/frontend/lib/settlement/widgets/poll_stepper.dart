import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../bloc/settlement_bloc.dart';
import '../models/rvm/settlement_proposal_vm.dart';
import '../../general/widgets/vote_dialog.dart';
import '../../widget_extensions.dart';
import '../bloc/settlement_actions.dart';
import '../bloc/settlement_result_vm.dart';
import '../models/im/decision_im.dart';
import '../../general/widgets/swipe_button.dart';

class PollStepper extends StatefulWidget {
  final SettlementProposalVm proposal;
  final GetAssessmentPollInfoSuccessVm info;
  final int currentBlock;
  final int endBlock;

  const PollStepper({
    super.key,
    required this.proposal,
    required this.info,
    required this.currentBlock,
    required this.endBlock,
  });

  @override
  State<PollStepper> createState() => _PollStepperState();
}

class _PollStepperState extends StateX<PollStepper> {
  late final _settlementBloc = use<SettlementBloc>();

  int _currentStep = 0;

  bool _checkButtonShouldBeEnabled() {
    var info = widget.info;
    return info.initBlock != null &&
        info.isDesignatedVerifier != null &&
        widget.currentBlock < widget.endBlock &&
        info.isDesignatedVerifier!;
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
      child: Stepper(
        currentStep: _currentStep,
        controlsBuilder: (context, details) => SwipeButton(
          key: ValueKey('${widget.info.userId} ${details.currentStep}'),
          text: 'Slide to vote',
          enabled: _checkButtonShouldBeEnabled(),
          swiped: false,
          onCompletedSwipe: () async {
            await showDialog(
              context: context,
              builder: (_) => VoteDialog<DecisionIm>(
                decisions: [
                  DecisionIm.accept,
                  DecisionIm.softDecline,
                  DecisionIm.hardDecline,
                ],
                getDisplayString: (decision) => decision.getString(),
                onVote: (decision, reason) async {
                  SettlementActionAwaitable<CastVoteResultVm> action =
                      details.currentStep == 0
                          ? CastVoteOffChain(
                              thingId: widget.proposal.thingId,
                              proposalId: widget.proposal.id,
                              decision: decision,
                              reason: reason,
                            )
                          : CastVoteOnChain(
                              thingId: widget.proposal.thingId,
                              proposalId: widget.proposal.id,
                              decision: decision,
                              reason: reason,
                            );

                  _settlementBloc.dispatch(action);
                  await action.result;
                },
              ),
            );

            return false;
          },
        ),
        onStepTapped: (value) => setState(() {
          _currentStep = value;
        }),
        steps: [
          Step(
            state: StepState.editing,
            title: Text(
              'Vote off-chain',
              style: GoogleFonts.philosopher(
                color: Color(0xffF8F9FA),
                fontSize: 16,
              ),
            ),
            content: Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: Text(
                'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor',
                style: GoogleFonts.raleway(
                  color: Colors.white,
                ),
              ),
            ),
            isActive: true,
          ),
          Step(
            state: StepState.editing,
            title: Text(
              'Vote on-chain',
              style: GoogleFonts.philosopher(
                color: Color(0xffF8F9FA),
                fontSize: 16,
              ),
            ),
            content: Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: Text(
                'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor',
                style: GoogleFonts.raleway(
                  color: Colors.white,
                ),
              ),
            ),
            isActive: true,
          ),
        ],
      ),
    );
  }
}
