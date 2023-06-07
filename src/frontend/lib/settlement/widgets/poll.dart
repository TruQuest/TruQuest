import 'dart:math';

import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

import '../../general/widgets/block_countdown.dart';
import '../../general/widgets/verifiers_table.dart';
import '../models/rvm/settlement_proposal_vm.dart';
import '../bloc/settlement_bloc.dart';
import '../bloc/settlement_actions.dart';
import '../../ethereum/bloc/ethereum_bloc.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';
import 'poll_stepper.dart';

class Poll extends StatefulWidget {
  final SettlementProposalVm proposal;

  const Poll({super.key, required this.proposal});

  @override
  State<Poll> createState() => _PollState();
}

class _PollState extends StateX<Poll> {
  late final _userBloc = use<UserBloc>();
  late final _settlementBloc = use<SettlementBloc>();
  late final _ethereumBloc = use<EthereumBloc>();

  String? _currentUserId;

  late GetAssessmentPollInfo _getInfoAction;

  @override
  void initState() {
    super.initState();

    _currentUserId = _userBloc.latestCurrentUser?.user.id;

    _settlementBloc.dispatch(GetVerifiers(proposalId: widget.proposal.id));

    _getInfoAction = GetAssessmentPollInfo(
      thingId: widget.proposal.thingId,
      proposalId: widget.proposal.id,
    );
    _settlementBloc.dispatch(_getInfoAction);
  }

  @override
  void didUpdateWidget(covariant Poll oldWidget) {
    super.didUpdateWidget(oldWidget);
    _currentUserId = _userBloc.latestCurrentUser?.user.id;
    _getInfoAction = GetAssessmentPollInfo(
      thingId: widget.proposal.thingId,
      proposalId: widget.proposal.id,
    );
    _settlementBloc.dispatch(_getInfoAction);
  }

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Expanded(
          child: StreamBuilder(
            stream: _settlementBloc.verifiers$,
            builder: (context, snapshot) {
              if (snapshot.data == null) {
                return Center(child: CircularProgressIndicator());
              }

              var verifiers = snapshot.data!.verifiers;

              return VerifiersTable(
                verifiers: verifiers,
                currentUserId: _currentUserId,
                onRefresh: () => _settlementBloc.dispatch(
                  GetVerifiers(proposalId: widget.proposal.id),
                ),
              );
            },
          ),
        ),
        Expanded(
          child: FutureBuilder(
            future: _getInfoAction.result,
            builder: (context, snapshot) {
              if (snapshot.data == null) {
                return Center(child: CircularProgressIndicator());
              }

              var info = snapshot.data!;

              return StreamBuilder(
                stream: _ethereumBloc.latestL1BlockNumber$,
                builder: (context, snapshot) {
                  if (snapshot.data == null) {
                    return Center(child: CircularProgressIndicator());
                  }

                  var latestBlockNumber = snapshot.data!.toDouble();
                  var startBlock = info.initBlock?.toDouble() ?? 0;
                  var endBlock = startBlock + info.durationBlocks;
                  var currentBlock = 0.0;
                  if (info.initBlock != null) {
                    currentBlock = min(
                      max(
                        latestBlockNumber,
                        info.latestBlockNumber,
                      ),
                      endBlock,
                    ).toDouble();
                  }

                  return Center(
                    child: Column(
                      children: [
                        Container(
                          margin: const EdgeInsets.only(top: 18),
                          decoration: BoxDecoration(
                            border: Border(
                              bottom: BorderSide(color: Colors.white),
                            ),
                          ),
                          padding: const EdgeInsets.fromLTRB(8, 0, 8, 6),
                          child: Text(
                            'Poll',
                            style: GoogleFonts.philosopher(
                              color: Colors.white,
                              fontSize: 24,
                            ),
                          ),
                        ),
                        SizedBox(height: 12),
                        Stack(
                          alignment: Alignment.center,
                          children: [
                            SleekCircularSlider(
                              min: startBlock,
                              max: endBlock,
                              initialValue: currentBlock,
                              appearance: CircularSliderAppearance(
                                size: 300,
                                customColors: CustomSliderColors(
                                  dotColor: Colors.transparent,
                                ),
                              ),
                              innerWidget: (_) => SizedBox.shrink(),
                            ),
                            if (info.initBlock != null)
                              BlockCountdown(
                                blocksLeft: (endBlock - currentBlock).toInt(),
                              ),
                            Positioned(
                              bottom: 20,
                              left: 0,
                              right: 0,
                              child: Row(
                                children: [
                                  Text(
                                    startBlock.toStringAsFixed(0),
                                    style: GoogleFonts.righteous(
                                      color: Colors.white,
                                      fontSize: 26,
                                    ),
                                  ),
                                  Spacer(),
                                  Text(
                                    endBlock.toStringAsFixed(0),
                                    style: GoogleFonts.righteous(
                                      color: Colors.white,
                                      fontSize: 26,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ],
                        ),
                        SizedBox(height: 12),
                        PollStepper(
                          proposal: widget.proposal,
                          info: info,
                          currentBlock: currentBlock.toInt(),
                          endBlock: endBlock.toInt(),
                        ),
                      ],
                    ),
                  );
                },
              );
            },
          ),
        ),
      ],
    );
  }
}
