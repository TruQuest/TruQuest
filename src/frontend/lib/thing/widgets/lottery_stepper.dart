import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

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
                'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor',
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
