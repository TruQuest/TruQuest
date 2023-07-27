import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../bloc/settlement_actions.dart';
import '../../general/utils/utils.dart';
import '../models/rvm/settlement_proposal_vm.dart';
import '../../general/widgets/swipe_button.dart';
import '../../widget_extensions.dart';
import '../bloc/settlement_bloc.dart';
import '../models/rvm/verifier_lottery_info_vm.dart';

// ignore: must_be_immutable
class LotteryStepper extends StatelessWidgetX {
  late final _settlementBloc = use<SettlementBloc>();

  final SettlementProposalVm proposal;
  final VerifierLotteryInfoVm info;
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
      currentBlock < endBlock;

  bool _checkButtonShouldBeSwiped(int step) =>
      step == -1 && info.alreadyClaimedASpot != null && info.alreadyClaimedASpot! ||
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
      child: Column(
        children: [
          Stepper(
            controlsBuilder: (context, details) =>
                info.userId != null && !proposal.isSubmitter(info.userId) && info.userIndexInThingVerifiersArray >= 0
                    ? SwipeButton(
                        key: ValueKey(info.userId),
                        text: 'Slide to claim',
                        enabled: _checkButtonShouldBeEnabled(-1),
                        swiped: _checkButtonShouldBeSwiped(-1),
                        onCompletedSwipe: () async {
                          bool success = await multiStageFlow(
                            context,
                            (ctx) => _settlementBloc.executeMultiStage(
                              ClaimLotterySpot(
                                thingId: proposal.thingId,
                                proposalId: proposal.id,
                                userIndexInThingVerifiersArray: info.userIndexInThingVerifiersArray,
                              ),
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
                  'Claim a spot',
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
          const SizedBox(height: 6),
          const Divider(
            color: Colors.white70,
            indent: 116,
            endIndent: 80,
          ),
          const SizedBox(height: 6),
          Stepper(
            controlsBuilder: (context, details) => info.userId != null && !proposal.isSubmitter(info.userId)
                ? SwipeButton(
                    key: ValueKey(info.userId),
                    text: 'Slide to join',
                    enabled: _checkButtonShouldBeEnabled(0),
                    swiped: _checkButtonShouldBeSwiped(0),
                    onCompletedSwipe: () async {
                      bool success = await multiStageFlow(
                        context,
                        (ctx) => _settlementBloc.executeMultiStage(
                          JoinLottery(
                            thingId: proposal.thingId,
                            proposalId: proposal.id,
                          ),
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
        ],
      ),
    );
  }
}
