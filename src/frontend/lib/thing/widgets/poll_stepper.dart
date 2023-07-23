import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../general/utils/utils.dart';
import '../../user/errors/wallet_locked_error.dart';
import '../bloc/thing_actions.dart';
import '../bloc/thing_result_vm.dart';
import '../../general/widgets/vote_dialog.dart';
import '../../widget_extensions.dart';
import '../bloc/thing_bloc.dart';
import '../models/im/decision_im.dart';
import '../models/rvm/thing_vm.dart';
import '../../general/widgets/swipe_button.dart';

class PollStepper extends StatefulWidget {
  final ThingVm thing;
  final GetAcceptancePollInfoSuccessVm info;
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

  bool _checkButtonShouldBeEnabled() =>
      widget.info.initBlock != null && widget.currentBlock < widget.endBlock;

  @override
  Widget buildX(BuildContext context) {
    return Theme(
      data: ThemeData(
        brightness: Brightness.dark,
        colorScheme: Theme.of(context).colorScheme.copyWith(
              brightness: Brightness.dark,
              secondary: const Color(0xffF8F9FA),
            ),
      ),
      child: Stepper(
        currentStep: _currentStep,
        controlsBuilder: (context, details) => widget.info.userId != null &&
                widget.info.userIndexInThingVerifiersArray >= 0
            ? SwipeButton(
                // @@NOTE: Without the key flutter would just reuse the same state object for all steps.
                key: ValueKey('${widget.info.userId} ${details.currentStep}'),
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
                          var action = CastVoteOffChain(
                            thingId: widget.thing.id,
                            decision: decision,
                            reason: reason,
                          );
                          _thingBloc.dispatch(action);

                          var failure = await action.result;
                          if (failure != null &&
                              failure.error is WalletLockedError) {
                            if (context.mounted) {
                              var unlocked = await showUnlockWalletDialog(
                                context,
                              );
                              if (unlocked) {
                                _thingBloc.dispatch(action);
                                failure = await action.result;
                              }
                            }
                          }
                        } else {
                          var action = CastVoteOnChain(
                            thingId: widget.thing.id,
                            userIndexInThingVerifiersArray:
                                widget.info.userIndexInThingVerifiersArray,
                            decision: decision,
                            reason: reason,
                          );
                          _thingBloc.dispatch(action);

                          var failure = await action.result;
                          if (failure != null &&
                              failure.error is WalletLockedError) {
                            if (context.mounted) {
                              var unlocked = await showUnlockWalletDialog(
                                context,
                              );
                              if (unlocked) {
                                _thingBloc.dispatch(action);
                                failure = await action.result;
                              }
                            }
                          }
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
