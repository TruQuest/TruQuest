import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../general/utils/utils.dart';
import '../bloc/thing_result_vm.dart';
import '../bloc/thing_bloc.dart';
import '../models/rvm/thing_vm.dart';
import '../../general/widgets/swipe_button.dart';
import '../../widget_extensions.dart';

// ignore: must_be_immutable
class LotteryStepper extends StatelessWidgetX {
  late final _thingBloc = use<ThingBloc>();

  final ThingVm thing;
  final GetVerifierLotteryInfoSuccessVm info;
  final int currentBlock;
  final int endBlock;

  LotteryStepper({
    super.key,
    required this.thing,
    required this.info,
    required this.currentBlock,
    required this.endBlock,
  });

  bool _checkButtonShouldBeEnabled() =>
      info.initBlock != null &&
      info.alreadyJoined != null &&
      !info.alreadyJoined! &&
      currentBlock < endBlock;

  bool _checkButtonShouldBeSwiped() =>
      info.alreadyJoined != null && info.alreadyJoined!;

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
        controlsBuilder: (context, details) =>
            info.userId != null && !thing.isSubmitter(info.userId)
                ? SwipeButton(
                    key: ValueKey(info.userId),
                    text: 'Slide to join',
                    enabled: _checkButtonShouldBeEnabled(),
                    swiped: _checkButtonShouldBeSwiped(),
                    onCompletedSwipe: () async {
                      bool success = await multiStageAction(
                        context,
                        (ctx) => _thingBloc.joinLottery(thing.id, ctx),
                      );

                      return success;
                    },
                  )
                : const SizedBox.shrink(),
        steps: [
          Step(
            title: Text(
              'Join lottery',
              style: GoogleFonts.philosopher(
                color: const Color(0xffF8F9FA),
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
