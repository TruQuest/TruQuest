import 'package:flutter/material.dart';

import 'swipe_button.dart';
import '../../widget_extensions.dart';

class LotteryStepper extends StatefulWidget {
  const LotteryStepper({super.key});

  @override
  State<LotteryStepper> createState() => _LotteryStepperState();
}

class _LotteryStepperState extends StateX<LotteryStepper> {
  int _currentStep = 0;

  @override
  Widget build(BuildContext context) {
    return Stepper(
      currentStep: _currentStep,
      controlsBuilder: (context, details) => SwipeButton(
        text: 'Slide to ${details.currentStep == 0 ? 'commit' : 'join'}',
        onCompletedSwipe: () async {
          await Future.delayed(Duration(seconds: 2));
          if (details.currentStep == 0) {
            details.onStepContinue!();
          }
          return true;
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
