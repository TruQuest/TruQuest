import 'dart:math';

import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

import '../../general/widgets/block_countdown.dart';
import '../../general/widgets/lottery_participants_table.dart';
import '../bloc/settlement_actions.dart';
import '../bloc/settlement_bloc.dart';
import '../models/rvm/settlement_proposal_vm.dart';
import '../../ethereum/bloc/ethereum_bloc.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';
import 'lottery_stepper.dart';

class Lottery extends StatefulWidget {
  final SettlementProposalVm proposal;

  const Lottery({super.key, required this.proposal});

  @override
  State<Lottery> createState() => _LotteryState();
}

class _LotteryState extends StateX<Lottery> {
  late final _userBloc = use<UserBloc>();
  late final _settlementBloc = use<SettlementBloc>();
  late final _ethereumBloc = use<EthereumBloc>();

  String? _currentUserId;

  @override
  void initState() {
    super.initState();

    _currentUserId = _userBloc.latestCurrentUser?.user.id;

    _settlementBloc.dispatch(
      GetVerifierLotteryParticipants(
        thingId: widget.proposal.thingId,
        proposalId: widget.proposal.id,
      ),
    );
    _settlementBloc.dispatch(
      GetVerifierLotteryInfo(
        thingId: widget.proposal.thingId,
        proposalId: widget.proposal.id,
      ),
    );
  }

  @override
  void didUpdateWidget(covariant Lottery oldWidget) {
    super.didUpdateWidget(oldWidget);
    _currentUserId = _userBloc.latestCurrentUser?.user.id;
    _settlementBloc.dispatch(
      GetVerifierLotteryInfo(
        thingId: widget.proposal.thingId,
        proposalId: widget.proposal.id,
      ),
    );
  }

  @override
  Widget buildX(BuildContext context) {
    return Row(
      children: [
        Expanded(
          child: StreamBuilder(
            stream: _settlementBloc.verifierLotteryParticipants$,
            builder: (context, snapshot) {
              if (snapshot.data == null) {
                return const Center(child: CircularProgressIndicator());
              }

              var entries = snapshot.data!.entries;

              return LotteryParticipantsTable(
                entries: entries,
                currentUserId: _currentUserId,
                onRefresh: () => _settlementBloc.dispatch(
                  GetVerifierLotteryParticipants(
                    thingId: widget.proposal.thingId,
                    proposalId: widget.proposal.id,
                  ),
                ),
              );
            },
          ),
        ),
        Expanded(
          child: StreamBuilder(
            stream: _settlementBloc.verifierLotteryInfo$,
            builder: (context, snapshot) {
              if (snapshot.data == null) {
                return const Center(child: CircularProgressIndicator());
              }

              var info = snapshot.data!;

              return StreamBuilder(
                stream: _ethereumBloc.latestL1BlockNumber$,
                initialData: info.latestL1BlockNumber,
                builder: (context, snapshot) {
                  var latestBlockNumber = snapshot.data!.toDouble();
                  var startBlock = info.initBlock?.abs().toDouble() ?? 0;
                  var endBlock = startBlock + info.durationBlocks;
                  var currentBlock = 0.0;
                  if (info.initBlock != null) {
                    currentBlock = min(latestBlockNumber, endBlock).toDouble();
                  }

                  return Center(
                    child: SingleChildScrollView(
                      child: Column(
                        children: [
                          Container(
                            margin: const EdgeInsets.only(top: 18),
                            decoration: const BoxDecoration(
                              border: Border(
                                bottom: BorderSide(color: Colors.white),
                              ),
                            ),
                            padding: const EdgeInsets.fromLTRB(8, 0, 8, 6),
                            child: Text(
                              'Lottery',
                              style: GoogleFonts.philosopher(
                                color: Colors.white,
                                fontSize: 24,
                              ),
                            ),
                          ),
                          const SizedBox(height: 12),
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
                                innerWidget: (_) => const SizedBox.shrink(),
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
                                    const Spacer(),
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
                          const SizedBox(height: 12),
                          LotteryStepper(
                            proposal: widget.proposal,
                            info: info,
                            currentBlock: currentBlock.toInt(),
                            endBlock: endBlock.toInt(),
                          ),
                        ],
                      ),
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
