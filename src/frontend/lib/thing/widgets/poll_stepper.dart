import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../bloc/thing_actions.dart';
import '../../general/utils/utils.dart';
import '../../general/widgets/vote_dialog.dart';
import '../../widget_extensions.dart';
import '../bloc/thing_bloc.dart';
import '../models/im/decision_im.dart';
import '../models/vm/validation_poll_info_vm.dart';
import '../models/vm/thing_vm.dart';
import '../../general/widgets/swipe_button.dart';

class PollStepper extends StatefulWidget {
  final ThingVm thing;
  final ValidationPollInfoVm info;
  final int currentBlock;
  final int endBlock;

  const PollStepper({
    super.key,
    required this.thing,
    required this.info,
    required this.currentBlock,
    required this.endBlock,
  });

  @override
  State<PollStepper> createState() => _PollStepperState();
}

class _PollStepperState extends StateX<PollStepper> {
  late final _thingBloc = use<ThingBloc>();

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
        controlsBuilder: (context, details) => widget.info.userId != null && widget.info.thingVerifiersArrayIndex >= 0
            ? SwipeButton.expand(
                // @@NOTE: Without the key flutter would just reuse the same state object for all steps.
                key: ValueKey(
                  '${details.currentStep}::${widget.info.userId}::${widget.currentBlock < widget.endBlock}',
                ),
                height: 50,
                enabled: _checkButtonShouldBeEnabled(),
                swiped: false,
                onTrackChild: const Text(
                  'Swipe to vote',
                  style: TextStyle(color: Colors.black54),
                ),
                onExpandingHandleChild: const Icon(
                  Icons.double_arrow_rounded,
                  color: Colors.white,
                ),
                color: Colors.red,
                disabledColor: Colors.blue[200]!,
                trackColor: Colors.grey[350]!,
                onFullSwipe: () async {
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
                            (ctx) => _thingBloc.executeMultiStage(
                              CastVoteOffChain(
                                thingId: widget.thing.id,
                                decision: decision,
                                reason: reason,
                              ),
                              ctx,
                            ),
                          );
                        } else {
                          await multiStageFlow(
                            context,
                            (ctx) => _thingBloc.executeMultiStage(
                              CastVoteOnChain(
                                thingId: widget.thing.id,
                                thingVerifiersArrayIndex: widget.info.thingVerifiersArrayIndex,
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
