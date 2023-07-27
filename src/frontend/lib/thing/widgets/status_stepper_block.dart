import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../general/utils/utils.dart';
import '../../general/contexts/document_view_context.dart';
import '../../general/widgets/swipe_button.dart';
import '../../user/bloc/user_bloc.dart';
import '../bloc/thing_actions.dart';
import '../bloc/thing_bloc.dart';
import '../models/rvm/thing_state_vm.dart';
import '../models/rvm/thing_vm.dart';
import '../../widget_extensions.dart';

// ignore: must_be_immutable
class StatusStepperBlock extends StatelessWidgetX {
  late final _userBloc = use<UserBloc>();
  late final _thingBloc = use<ThingBloc>();
  late final _documentViewContext = useScoped<DocumentViewContext>();

  String? _currentUserId;
  late ThingVm _thing;
  late int _currentStep;

  StatusStepperBlock({super.key});

  void _setup() {
    _currentUserId = _userBloc.latestCurrentUser?.id;
    _thing = _documentViewContext.thing!;
    if (_thing.state.index <= ThingStateVm.fundedAndVerifierLotteryInitiated.index) {
      _currentStep = _thing.state.index + 1;
    } else if (_thing.state == ThingStateVm.verifierLotteryFailed ||
        _thing.state == ThingStateVm.verifiersSelectedAndPollInitiated) {
      _currentStep = ThingStateVm.fundedAndVerifierLotteryInitiated.index + 2;
    } else if (_thing.state == ThingStateVm.consensusNotReached ||
        _thing.state == ThingStateVm.declined ||
        _thing.state == ThingStateVm.awaitingSettlement) {
      _currentStep = ThingStateVm.fundedAndVerifierLotteryInitiated.index + 3;
    } else {
      _currentStep = ThingStateVm.fundedAndVerifierLotteryInitiated.index + 4;
    }
  }

  List<Step> _buildFinalSteps() {
    if (_thing.state.index <= ThingStateVm.fundedAndVerifierLotteryInitiated.index) {
      return [];
    } else if (_thing.state == ThingStateVm.verifierLotteryFailed) {
      return [
        Step(
          title: Text(
            'Lottery failed',
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
      ];
    }

    var steps = [
      Step(
        title: Text(
          'Poll in progress',
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
    ];

    if (_thing.state == ThingStateVm.verifiersSelectedAndPollInitiated) {
      return steps;
    } else if (_thing.state == ThingStateVm.awaitingSettlement || _thing.state == ThingStateVm.settled) {
      return [
        ...steps,
        Step(
          title: Text(
            'Awaiting settlement',
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
        Step(
          title: Text(
            'Settled',
            style: GoogleFonts.philosopher(
              color: const Color(0xffF8F9FA),
              fontSize: 16,
            ),
          ),
          content: InkWell(
            onTap: () {},
            child: Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: Text(
                'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor',
                style: GoogleFonts.raleway(
                  color: Colors.white,
                ),
              ),
            ),
          ),
          isActive: true,
        ),
      ];
    } else if (_thing.state == ThingStateVm.declined) {
      return [
        ...steps,
        Step(
          title: Text(
            'Declined',
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
      ];
    }

    return [
      ...steps,
      Step(
        title: Text(
          'Consensus not reached',
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
    ];
  }

  bool _checkShouldBeEnabled(int step) =>
      (step == 0 || step == 1) && _thing.state == ThingStateVm.draft ||
      step == 2 && _thing.state == ThingStateVm.awaitingFunding && !_thing.fundedAwaitingConfirmation!;

  bool _checkShouldBeSwiped(int step) =>
      (step == 0 || step == 1) && _thing.state.index > ThingStateVm.draft.index ||
      step == 2 &&
          (_thing.state.index > ThingStateVm.awaitingFunding.index ||
              _thing.state == ThingStateVm.awaitingFunding && _thing.fundedAwaitingConfirmation!);

  @override
  Widget buildX(BuildContext context) {
    _setup();
    return Theme(
      data: getThemeDataForSteppers(context),
      child: Stepper(
        currentStep: _currentStep,
        controlsBuilder: (context, details) {
          var step = details.currentStep;
          if (_thing.isSubmitter(_currentUserId) && step <= 2) {
            return SwipeButton(
              key: ValueKey(step),
              text: 'Swipe to ${step == 0 ? 'edit' : step == 1 ? 'submit' : 'fund'}',
              enabled: _checkShouldBeEnabled(step),
              swiped: _checkShouldBeSwiped(step),
              onCompletedSwipe: () async {
                if (step == 0) {
                  return true;
                } else if (step == 1) {
                  var success = await _thingBloc.execute<bool>(
                    SubmitNewThing(thingId: _thing.id),
                  );
                  return success.isTrue;
                }

                // ignore: use_build_context_synchronously
                bool success = await multiStageFlow(
                  context,
                  (ctx) => _thingBloc.executeMultiStage(
                    FundThing(
                      thingId: _thing.id,
                      signature: _documentViewContext.signature!,
                    ),
                    ctx,
                  ),
                );

                return success;
              },
            );
          }

          return const SizedBox.shrink();
        },
        steps: [
          Step(
            title: Text(
              _thing.isSubmitter(_currentUserId) ? 'Draft' : 'Draft created',
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
          Step(
            title: Text(
              _thing.isSubmitter(_currentUserId) ? 'Submit' : 'Awaiting submission',
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
          Step(
            title: Text(
              _thing.isSubmitter(_currentUserId) ? 'Fund' : 'Awaiting funding',
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
          Step(
            title: Text(
              'Lottery in progress',
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
          ..._buildFinalSteps(),
        ],
      ),
    );
  }
}
