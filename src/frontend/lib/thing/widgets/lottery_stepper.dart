import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../bloc/thing_actions.dart';
import '../../general/utils/utils.dart';
import '../bloc/thing_bloc.dart';
import '../models/vm/thing_vm.dart';
import '../../general/widgets/swipe_button.dart';
import '../../widget_extensions.dart';
import '../models/vm/verifier_lottery_info_vm.dart';

// ignore: must_be_immutable
class LotteryStepper extends StatelessWidgetX {
  late final _thingBloc = use<ThingBloc>();

  final ThingVm thing;
  final VerifierLotteryInfoVm info;
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
      info.initBlock! > 0 &&
      info.alreadyJoined != null &&
      !info.alreadyJoined! &&
      currentBlock < endBlock;

  bool _checkButtonShouldBeSwiped() => info.alreadyJoined != null && info.alreadyJoined!;

  @override
  Widget buildX(BuildContext context) {
    return Theme(
      data: getThemeDataForSteppers(context),
      child: Stepper(
        controlsBuilder: (context, details) => info.userId != null && !thing.isSubmitter(info.userId)
            ? SwipeButton(
                // @@NOTE: We want the button to:
                // - Update 'enabled' and 'swiped' to the values corresponding to the new user on user change.
                // - Not change 'enabled' and 'swiped' when an action is in progress (e.g. new block gets mined while
                //   we wait for a user to sign an operation), since we want it to remain disabled and swiped until the action
                //   is complete.
                // - Get disabled once the lottery is over.
                key: ValueKey('${info.userId}::${currentBlock < endBlock}::${info.alreadyJoined}'),
                text: 'Slide to join',
                enabled: _checkButtonShouldBeEnabled(),
                swiped: _checkButtonShouldBeSwiped(),
                onCompletedSwipe: () async {
                  bool success = await multiStageFlow(
                    context,
                    (ctx) => _thingBloc.executeMultiStage(
                      JoinLottery(thingId: thing.id),
                      ctx,
                    ),
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
