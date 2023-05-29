import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../general/contexts/document_view_context.dart';
import '../../general/widgets/swipe_button.dart';
import '../bloc/thing_actions.dart';
import '../bloc/thing_bloc.dart';
import '../bloc/thing_result_vm.dart';
import '../models/rvm/thing_state_vm.dart';
import '../models/rvm/thing_vm.dart';
import '../../widget_extensions.dart';

class StatusStepperBlock extends StatefulWidget {
  const StatusStepperBlock({super.key});

  @override
  State<StatusStepperBlock> createState() => _StatusStepperBlockState();
}

class _StatusStepperBlockState extends StateX<StatusStepperBlock> {
  late final _thingBloc = use<ThingBloc>();

  late DocumentViewContext _documentViewContext;
  late ThingVm _thing;

  late int _currentStep;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    _documentViewContext = useScoped<DocumentViewContext>();
    _thing = _documentViewContext.thing!;
    if (_thing.state.index <=
        ThingStateVm.verifiersSelectedAndPollInitiated.index) {
      _currentStep = _thing.state.index + 1;
    } else if (_thing.state == ThingStateVm.consensusNotReached ||
        _thing.state == ThingStateVm.declined ||
        _thing.state == ThingStateVm.awaitingSettlement) {
      _currentStep = ThingStateVm.verifiersSelectedAndPollInitiated.index + 2;
    } else {
      _currentStep = ThingStateVm.verifiersSelectedAndPollInitiated.index + 3;
    }
  }

  List<Step> _buildFinalSteps() {
    if (_thing.state == ThingStateVm.awaitingSettlement ||
        _thing.state == ThingStateVm.settled) {
      return [
        Step(
          title: Text(
            'Awaiting settlement',
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
            'Settled',
            style: GoogleFonts.philosopher(
              color: Color(0xffF8F9FA),
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
        Step(
          title: Text(
            'Declined',
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
      ];
    } else if (_thing.state == ThingStateVm.consensusNotReached) {
      return [
        Step(
          title: Text(
            'Consensus not reached',
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
      ];
    }

    return [];
  }

  bool _checkShouldBeEnabled(int step) =>
      (step == 0 || step == 1) && _thing.state == ThingStateVm.draft ||
      step == 2 &&
          _thing.state == ThingStateVm.awaitingFunding &&
          !_thing.fundedAwaitingConfirmation!;

  bool _checkShouldBeSwiped(int step) =>
      (step == 0 || step == 1) &&
          _thing.state.index > ThingStateVm.draft.index ||
      step == 2 &&
          (_thing.state.index > ThingStateVm.awaitingFunding.index ||
              _thing.state == ThingStateVm.awaitingFunding &&
                  _thing.fundedAwaitingConfirmation!);

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
        controlsBuilder: (context, details) {
          var step = details.currentStep;
          if (step <= 2) {
            return SwipeButton(
              key: ValueKey(step),
              text:
                  'Swipe to ${step == 0 ? 'edit' : step == 1 ? 'submit' : 'fund'}',
              enabled: _checkShouldBeEnabled(step),
              swiped: _checkShouldBeSwiped(step),
              onCompletedSwipe: () async {
                if (step == 0) {
                  return true;
                } else if (step == 1) {
                  var action = SubmitNewThing(thing: _thing);
                  _thingBloc.dispatch(action);

                  SubmitNewThingFailureVm? failure = await action.result;
                  if (failure == null) {
                    _thingBloc.dispatch(GetThing(thingId: _thing.id));
                  }

                  return failure == null;
                }

                var action = FundThing(
                  thing: _thing,
                  signature: _documentViewContext.signature!,
                );
                _thingBloc.dispatch(action);

                FundThingFailureVm? failure = await action.result;
                return failure == null;
              },
            );
          }

          return SizedBox.shrink();
        },
        steps: [
          Step(
            title: Text(
              'Draft',
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
              'Submit',
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
              'Fund',
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
              'Lottery in progress',
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
              'Poll in progress',
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
          ..._buildFinalSteps(),
        ],
      ),
    );
  }
}
