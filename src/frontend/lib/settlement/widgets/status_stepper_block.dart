import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../bloc/settlement_actions.dart';
import '../models/rvm/settlement_proposal_state_vm.dart';
import '../models/rvm/settlement_proposal_vm.dart';
import '../../general/contexts/document_view_context.dart';
import '../../general/widgets/swipe_button.dart';
import '../../widget_extensions.dart';
import '../bloc/settlement_bloc.dart';

class StatusStepperBlock extends StatefulWidget {
  const StatusStepperBlock({super.key});

  @override
  State<StatusStepperBlock> createState() => _StatusStepperBlockState();
}

class _StatusStepperBlockState extends StateX<StatusStepperBlock> {
  late final _settlementBloc = use<SettlementBloc>();

  late DocumentViewContext _documentViewContext;
  late SettlementProposalVm _proposal;

  late int _currentStep;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    _documentViewContext = useScoped<DocumentViewContext>();
    _proposal = _documentViewContext.proposal!;
    if (_proposal.state.index <=
        SettlementProposalStateVm.fundedAndVerifierLotteryInitiated.index) {
      _currentStep = _proposal.state.index + 1;
    } else if (_proposal.state ==
            SettlementProposalStateVm.verifierLotteryFailed ||
        _proposal.state ==
            SettlementProposalStateVm.verifiersSelectedAndPollInitiated) {
      _currentStep =
          SettlementProposalStateVm.fundedAndVerifierLotteryInitiated.index + 2;
    } else {
      _currentStep =
          SettlementProposalStateVm.fundedAndVerifierLotteryInitiated.index + 3;
    }
  }

  List<Step> _buildFinalSteps() {
    if (_proposal.state.index <=
        SettlementProposalStateVm.fundedAndVerifierLotteryInitiated.index) {
      return [];
    } else if (_proposal.state ==
        SettlementProposalStateVm.verifierLotteryFailed) {
      return [
        Step(
          title: Text(
            'Lottery failed',
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

    var steps = [
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
    ];

    if (_proposal.state ==
        SettlementProposalStateVm.verifiersSelectedAndPollInitiated) {
      return steps;
    } else if (_proposal.state == SettlementProposalStateVm.accepted) {
      return [
        ...steps,
        Step(
          title: Text(
            'Accepted',
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
    } else if (_proposal.state == SettlementProposalStateVm.softDeclined ||
        _proposal.state == SettlementProposalStateVm.hardDeclined) {
      return [
        ...steps,
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
    }

    return [
      ...steps,
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

  bool _checkShouldBeEnabled(int step) =>
      (step == 0 || step == 1) &&
          _proposal.state == SettlementProposalStateVm.draft ||
      step == 2 &&
          _proposal.state == SettlementProposalStateVm.awaitingFunding &&
          _proposal.canBeFunded!;

  bool _checkShouldBeSwiped(int step) =>
      (step == 0 || step == 1) &&
          _proposal.state.index > SettlementProposalStateVm.draft.index ||
      step == 2 &&
          (_proposal.state.index >
                  SettlementProposalStateVm.awaitingFunding.index ||
              _proposal.state == SettlementProposalStateVm.awaitingFunding &&
                  !_proposal.canBeFunded!);

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
                  var action = SubmitNewSettlementProposal(
                    proposalId: _proposal.id,
                  );
                  _settlementBloc.dispatch(action);

                  var failure = await action.result;
                  if (failure == null) {
                    _settlementBloc.dispatch(
                      GetSettlementProposal(proposalId: _proposal.id),
                    );
                  }

                  return failure == null;
                }

                var action = FundSettlementProposal(
                  thingId: _proposal.thingId,
                  proposalId: _proposal.id,
                  signature: _documentViewContext.signature!,
                );
                _settlementBloc.dispatch(action);

                var failure = await action.result;
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
          ..._buildFinalSteps(),
        ],
      ),
    );
  }
}
