import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../bloc/settlement_actions.dart';
import '../../general/utils/utils.dart';
import '../bloc/settlement_bloc.dart';
import '../models/rvm/assessment_poll_info_vm.dart';
import '../models/rvm/settlement_proposal_vm.dart';
import '../../general/widgets/vote_dialog.dart';
import '../../widget_extensions.dart';
import '../models/im/decision_im.dart';
import '../../general/widgets/swipe_button.dart';

class PollStepper extends StatefulWidget {
  final SettlementProposalVm proposal;
  final AssessmentPollInfoVm info;
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

  @override
  void didUpdateWidget(covariant PollStepper oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.info.userId != oldWidget.info.userId) {
      _currentStep = 0;
    }
  }

  bool _checkButtonShouldBeEnabled() => widget.info.initBlock != null && widget.currentBlock < widget.endBlock;

  @override
  Widget buildX(BuildContext context) {
    return Theme(
      data: getThemeDataForSteppers(context),
      child: Stepper(
        currentStep: _currentStep,
        controlsBuilder: (context, details) =>
            widget.info.userId != null && widget.info.settlementProposalVerifiersArrayIndex >= 0
                ? SwipeButton(
                    key: ValueKey(
                      '${details.currentStep}::${widget.info.userId}::${widget.currentBlock < widget.endBlock}',
                    ),
                    text: 'Slide to vote',
                    enabled: _checkButtonShouldBeEnabled(),
                    swiped: false,
                    onCompletedSwipe: () async {
                      await showDialog(
                        context: context,
                        builder: (_) => VoteDialog<DecisionIm>(
                          decisions: const [
                            DecisionIm.accept,
                            DecisionIm.softDecline,
                            DecisionIm.hardDecline,
                          ],
                          getDisplayString: (decision) => decision.getString(),
                          onVote: (decision, reason) async {
                            if (details.currentStep == 0) {
                              await multiStageOffChainFlow(
                                context,
                                (ctx) => _settlementBloc.executeMultiStage(
                                  CastVoteOffChain(
                                    thingId: widget.proposal.thingId,
                                    proposalId: widget.proposal.id,
                                    decision: decision,
                                    reason: reason,
                                  ),
                                  ctx,
                                ),
                              );
                            } else {
                              await multiStageFlow(
                                context,
                                (ctx) => _settlementBloc.executeMultiStage(
                                  CastVoteOnChain(
                                    thingId: widget.proposal.thingId,
                                    proposalId: widget.proposal.id,
                                    settlementProposalVerifiersArrayIndex:
                                        widget.info.settlementProposalVerifiersArrayIndex,
                                    decision: decision,
                                    reason: reason,
                                  ),
                                  ctx,
                                ),
                              );
                            }
                          },
                        ),
                      );

                      return false;
                    },
                  )
                : const SizedBox.shrink(),
        onStepTapped: (value) => setState(() => _currentStep = value),
        steps: [
          Step(
            state: StepState.editing,
            title: Text(
              'Vote off-chain',
              style: GoogleFonts.philosopher(
                color: const Color(0xffF8F9FA),
                fontSize: 16,
              ),
            ),
            content: Container(
              width: double.infinity,
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
                color: const Color(0xffF8F9FA),
                fontSize: 16,
              ),
            ),
            content: Container(
              width: double.infinity,
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
