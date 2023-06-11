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

  bool _checkButtonShouldBeEnabled() {
    var info = widget.info;
    return info.initBlock != null &&
        info.alreadyJoined != null &&
        !info.alreadyJoined! &&
        widget.currentBlock < widget.endBlock;
  }

  bool _checkButtonShouldBeSwiped() {
    var info = widget.info;
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
        controlsBuilder: (context, details) => SwipeButton(
          key: ValueKey(widget.info.userId),
          text: 'Slide to join',
          enabled: _checkButtonShouldBeEnabled(),
          swiped: _checkButtonShouldBeSwiped(),
          onCompletedSwipe: () async {
            var action = JoinLottery(thingId: widget.thing.id);
            _thingBloc.dispatch(action);

            var failure = await action.result;
            return failure == null;
          },
        ),
        steps: [
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
