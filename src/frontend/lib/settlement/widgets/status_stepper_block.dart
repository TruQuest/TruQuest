import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../general/utils/utils.dart';
import '../../user/bloc/user_bloc.dart';
import '../bloc/settlement_actions.dart';
import '../models/vm/settlement_proposal_state_vm.dart';
import '../models/vm/settlement_proposal_vm.dart';
import '../../general/contexts/document_view_context.dart';
import '../../general/widgets/swipe_button.dart';
import '../../widget_extensions.dart';
import '../bloc/settlement_bloc.dart';

// ignore: must_be_immutable
class StatusStepperBlock extends StatelessWidgetX {
  late final _userBloc = use<UserBloc>();
  late final _settlementBloc = use<SettlementBloc>();
  late final _documentViewContext = useScoped<DocumentViewContext>();

  String? _currentUserId;
  late SettlementProposalVm _proposal;
  late int _currentStep;

  StatusStepperBlock({super.key});

  void _setup() {
    _currentUserId = _userBloc.latestCurrentUser?.id;
    _proposal = _documentViewContext.proposal!;
    if (_proposal.state.index <= SettlementProposalStateVm.fundedAndVerifierLotteryInitiated.index) {
      _currentStep = _proposal.state.index + 1;
    } else if (_proposal.state == SettlementProposalStateVm.verifierLotteryFailed ||
        _proposal.state == SettlementProposalStateVm.verifiersSelectedAndPollInitiated) {
      _currentStep = SettlementProposalStateVm.fundedAndVerifierLotteryInitiated.index + 2;
    } else {
      _currentStep = SettlementProposalStateVm.fundedAndVerifierLotteryInitiated.index + 3;
    }
  }

  List<Step> _buildFinalSteps() {
    if (_proposal.state.index <= SettlementProposalStateVm.fundedAndVerifierLotteryInitiated.index) {
      return [];
    } else if (_proposal.state == SettlementProposalStateVm.verifierLotteryFailed) {
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

    if (_proposal.state == SettlementProposalStateVm.verifiersSelectedAndPollInitiated) {
      return steps;
    } else if (_proposal.state == SettlementProposalStateVm.accepted) {
      return [
        ...steps,
        Step(
          title: Text(
            'Accepted',
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
    } else if (_proposal.state == SettlementProposalStateVm.declined) {
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
      (step == 0 || step == 1) && _proposal.state == SettlementProposalStateVm.draft ||
      step == 2 && _proposal.state == SettlementProposalStateVm.awaitingFunding && _proposal.canBeFunded!;

  bool _checkShouldBeSwiped(int step) =>
      (step == 0 || step == 1) && _proposal.state.index > SettlementProposalStateVm.draft.index ||
      step == 2 &&
          (_proposal.state.index > SettlementProposalStateVm.awaitingFunding.index ||
              _proposal.state == SettlementProposalStateVm.awaitingFunding && !_proposal.canBeFunded!);

  @override
  Widget buildX(BuildContext context) {
    _setup();
    return Theme(
      data: getThemeDataForSteppers(context),
      child: Stepper(
        currentStep: _currentStep,
        controlsBuilder: (context, details) {
          var step = details.currentStep;
          if (_proposal.isSubmitter(_currentUserId) && step <= 2) {
            return SwipeButton.expand(
              key: ValueKey(step),
              height: 50,
              enabled: _checkShouldBeEnabled(step),
              swiped: _checkShouldBeSwiped(step),
              onTrackChild: Text(
                'Swipe to ${step == 0 ? 'edit' : step == 1 ? 'submit' : 'fund'}',
                style: const TextStyle(color: Colors.black54),
              ),
              onExpandingHandleChild: const Icon(
                Icons.double_arrow_rounded,
                color: Colors.white,
              ),
              color: Colors.red,
              disabledColor: Colors.blue[200]!,
              trackColor: Colors.grey[350]!,
              onFullSwipe: () async {
                if (step == 0) {
                  return true;
                } else if (step == 1) {
                  var success = await _settlementBloc.execute<bool>(
                    SubmitNewSettlementProposal(proposalId: _proposal.id),
                  );
                  return success.isTrue;
                }

                // ignore: use_build_context_synchronously
                bool success = await multiStageFlow(
                  context,
                  (ctx) => _settlementBloc.executeMultiStage(
                    FundSettlementProposal(
                      thingId: _proposal.thingId,
                      proposalId: _proposal.id,
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
              _proposal.isSubmitter(_currentUserId) ? 'Draft' : 'Draft created',
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
              _proposal.isSubmitter(_currentUserId) ? 'Submit' : 'Awaiting submission',
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
              _proposal.isSubmitter(_currentUserId) ? 'Fund' : 'Awaiting funding',
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
