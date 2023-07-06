import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../models/rvm/settlement_proposal_vm.dart';
import '../../general/widgets/swipe_button.dart';
import '../../widget_extensions.dart';
import '../bloc/settlement_actions.dart';
import '../bloc/settlement_bloc.dart';
import '../bloc/settlement_result_vm.dart';

// ignore: must_be_immutable
class LotteryStepper extends StatelessWidgetX {
  late final _settlementBloc = use<SettlementBloc>();

  final SettlementProposalVm proposal;
  final GetVerifierLotteryInfoSuccessVm info;
  final int currentBlock;
  final int endBlock;

  LotteryStepper({
    super.key,
    required this.proposal,
    required this.info,
    required this.currentBlock,
    required this.endBlock,
  });

  bool _checkButtonShouldBeEnabled(int step) =>
      info.initBlock != null &&
      info.alreadyJoined != null &&
      !info.alreadyJoined! &&
      info.alreadyClaimedASpot != null &&
      !info.alreadyClaimedASpot! &&
      currentBlock < endBlock &&
      (step == 0 || info.userIndexInThingVerifiersArray >= 0);

  bool _checkButtonShouldBeSwiped(int step) =>
      step == -1 &&
          info.alreadyClaimedASpot != null &&
          info.alreadyClaimedASpot! ||
      info.alreadyJoined != null && info.alreadyJoined!;

  @override
  Widget buildX(BuildContext context) {
    return Theme(
      data: ThemeData(
        brightness: Brightness.dark,
        colorScheme: Theme.of(context).colorScheme.copyWith(
              brightness: Brightness.dark,
              secondary: Color(0xffF8F9FA),
            ),
      ),
      child: Column(
        children: [
          Stepper(
            controlsBuilder: (context, details) =>
                info.userId != null && !proposal.isSubmitter(info.userId)
                    ? SwipeButton(
                        key: ValueKey(info.userId),
                        text: 'Slide to claim',
                        enabled: _checkButtonShouldBeEnabled(-1),
                        swiped: _checkButtonShouldBeSwiped(-1),
                        onCompletedSwipe: () async {
                          var action = ClaimLotterySpot(
                            thingId: proposal.thingId,
                            proposalId: proposal.id,
                            userIndexInThingVerifiersArray:
                                info.userIndexInThingVerifiersArray,
                          );
                          _settlementBloc.dispatch(action);

                          var failure = await action.result;
                          return failure == null;
                        },
                      )
                    : SizedBox.shrink(),
            steps: [
              Step(
                title: Text(
                  'Claim a spot',
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
          SizedBox(height: 6),
          Divider(
            color: Colors.white70,
            indent: 116,
            endIndent: 80,
          ),
          SizedBox(height: 6),
          Stepper(
            controlsBuilder: (context, details) =>
                info.userId != null && !proposal.isSubmitter(info.userId)
                    ? SwipeButton(
                        key: ValueKey(info.userId),
                        text: 'Slide to join',
                        enabled: _checkButtonShouldBeEnabled(0),
                        swiped: _checkButtonShouldBeSwiped(0),
                        onCompletedSwipe: () async {
                          var action = JoinLottery(
                            thingId: proposal.thingId,
                            proposalId: proposal.id,
                          );
                          _settlementBloc.dispatch(action);

                          var failure = await action.result;
                          return failure == null;
                        },
                      )
                    : SizedBox.shrink(),
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
        ],
      ),
    );
  }
}
