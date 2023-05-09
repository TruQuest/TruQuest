import 'package:flutter/material.dart';

import '../bloc/thing_result_vm.dart';
import '../bloc/thing_actions.dart';
import '../bloc/thing_bloc.dart';
import '../models/rvm/thing_vm.dart';
import '../../general/widgets/swipe_button.dart';
import '../../widget_extensions.dart';

class LotteryStepper extends StatefulWidget {
  final ThingVm thing;
  final GetVerifierLotteryInfoSuccessVm info;
  final int currentBlock;
  final int endBlock;

  const LotteryStepper({
    super.key,
    required this.thing,
    required this.info,
    required this.currentBlock,
    required this.endBlock,
  });

  @override
  State<LotteryStepper> createState() => _LotteryStepperState();
}

class _LotteryStepperState extends StateX<LotteryStepper> {
  late final _thingBloc = use<ThingBloc>();

  int _currentStep = 0;

  bool _checkButtonShouldBeEnabled(int stepIndex) {
    var info = widget.info;
    if (stepIndex == 0) {
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
    if (stepIndex == 0) {
      return info.alreadyPreJoined != null && info.alreadyPreJoined!;
    }

    return info.alreadyJoined != null && info.alreadyJoined!;
  }

  @override
  Widget build(BuildContext context) {
    return Stepper(
      currentStep: _currentStep,
      controlsBuilder: (context, details) => SwipeButton(
        text: 'Slide to ${details.currentStep == 0 ? 'commit' : 'join'}',
        enabled: _checkButtonShouldBeEnabled(details.currentStep),
        swiped: _checkButtonShouldBeSwiped(details.currentStep),
        onCompletedSwipe: () async {
          if (details.currentStep == 0) {
            var action = PreJoinLottery(thingId: widget.thing.id);
            _thingBloc.dispatch(action);

            var error = await action.result;
            if (error == null) {
              details.onStepContinue!();
              return true;
            }

            return false;
          }

          var action = JoinLottery(thingId: widget.thing.id);
          _thingBloc.dispatch(action);

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
    );
  }
}
