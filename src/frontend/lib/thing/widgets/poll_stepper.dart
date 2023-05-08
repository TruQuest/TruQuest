import 'package:flutter/material.dart';

import '../bloc/thing_actions.dart';
import '../bloc/thing_result_vm.dart';
import '../../general/widgets/vote_dialog.dart';
import '../../widget_extensions.dart';
import '../bloc/thing_bloc.dart';
import '../models/im/decision_im.dart';
import '../models/rvm/thing_vm.dart';
import 'swipe_button.dart';

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

  bool _checkButtonShouldBeEnabled() {
    var info = widget.info;
    return info.initBlock != null &&
        info.isDesignatedVerifier != null &&
        widget.currentBlock < widget.endBlock &&
        info.isDesignatedVerifier!;
  }

  @override
  Widget build(BuildContext context) {
    return Stepper(
      currentStep: _currentStep,
      controlsBuilder: (context, details) => SwipeButton(
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
                ThingActionAwaitable<CastVoteResultVm> action =
                    details.currentStep == 0
                        ? CastVoteOffChain(
                            thingId: widget.thing.id,
                            decision: decision,
                            reason: reason,
                          )
                        : CastVoteOnChain(
                            thingId: widget.thing.id,
                            decision: decision,
                            reason: reason,
                          );

                _thingBloc.dispatch(action);
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
          title: Text('Vote off-chain'),
          content: Padding(
            padding: const EdgeInsets.only(bottom: 12),
            child: Text(
                'Voting off-chain means that there won\'t be a blockchain transaction. Instead, your signed vote will be stored in IPFS.'),
          ),
        ),
        Step(
          state: StepState.editing,
          title: Text('Vote on-chain'),
          content: Padding(
            padding: const EdgeInsets.only(bottom: 12),
            child: Text(
                'On-chain vote supersedes any off-chain votes, even later ones. So one on-chain vote could only be overwritten by another one.'),
          ),
        ),
      ],
    );
  }
}
