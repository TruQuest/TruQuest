import 'dart:math';

import 'package:flutter/material.dart';
import 'package:auto_size_text/auto_size_text.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

import '../../general/widgets/block_countdown.dart';
import '../../general/widgets/clipped_block_number_container.dart';
import '../../general/widgets/corner_banner.dart';
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

  @override
  void initState() {
    super.initState();
    _settlementBloc.dispatch(
      GetVerifierLotteryParticipants(proposalId: widget.proposal.id),
    );
  }

  @override
  Widget build(BuildContext context) {
    return StreamBuilder(
      stream: _userBloc.currentUser$,
      builder: (context, snapshot) {
        var user = snapshot.data?.user;
        _settlementBloc.dispatch(GetVerifierLotteryInfo(
          thingId: widget.proposal.thingId,
          proposalId: widget.proposal.id,
        ));

        return Row(
          children: [
            Expanded(
              child: StreamBuilder(
                stream: _settlementBloc.verifierLotteryParticipants$,
                builder: (context, snapshot) {
                  if (snapshot.data == null) {
                    return Center(child: CircularProgressIndicator());
                  }

                  var entries = snapshot.data!.entries;

                  return Column(
                    children: [
                      Padding(
                        padding: const EdgeInsets.only(top: 16),
                        child: Stack(
                          children: [
                            Card(
                              margin: EdgeInsets.zero,
                              color: Colors.white,
                              elevation: 25,
                              shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(12),
                              ),
                              child: Padding(
                                padding: const EdgeInsets.only(
                                  left: 150,
                                  right: 16,
                                ),
                                child: SizedBox(
                                  width: 380,
                                  height: 80,
                                  child: Column(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    crossAxisAlignment:
                                        CrossAxisAlignment.start,
                                    children: [
                                      AutoSizeText(
                                        'User Id',
                                        style: TextStyle(
                                          color: Colors.black,
                                          fontSize: 18,
                                        ),
                                        maxLines: 1,
                                        overflow: TextOverflow.ellipsis,
                                      ),
                                      SizedBox(height: 8),
                                      Text(
                                        'Commitment',
                                        style: TextStyle(
                                          color: Colors.black54,
                                        ),
                                      ),
                                      SizedBox(height: 8),
                                      Text(
                                        'Nonce',
                                        style: TextStyle(
                                          color: Colors.black54,
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                              ),
                            ),
                            ClippedBlockNumberContainer(
                              color: Colors.indigo[900]!,
                              height: 80,
                              child: Text(
                                'Block',
                                style: TextStyle(
                                  color: Colors.white,
                                  fontSize: 20,
                                ),
                              ),
                            ),
                            CornerBanner(
                              position: Alignment.topLeft,
                              size: 40,
                              cornerRadius: 12,
                              color: Colors.white,
                              child: Icon(
                                Icons.numbers,
                                size: 14,
                              ),
                            )
                          ],
                        ),
                      ),
                      Expanded(
                        child: ListView.builder(
                          // @@??: ListView forces cross-axis stretch?
                          itemCount: entries.length,
                          itemBuilder: (context, index) {
                            var entry = entries[index];
                            return Center(
                              child: Padding(
                                padding: EdgeInsets.only(
                                  top: index == 0 ? 16 : 8,
                                  bottom: 8,
                                ),
                                child: Stack(
                                  children: [
                                    Card(
                                      margin: EdgeInsets.zero,
                                      color: Colors.white,
                                      elevation: 15,
                                      shape: RoundedRectangleBorder(
                                        borderRadius: BorderRadius.circular(12),
                                      ),
                                      child: Padding(
                                        padding: const EdgeInsets.only(
                                          left: 150,
                                          right: 16,
                                        ),
                                        child: SizedBox(
                                          width: 350,
                                          height: 120,
                                          child: Column(
                                            mainAxisAlignment:
                                                MainAxisAlignment.center,
                                            crossAxisAlignment:
                                                CrossAxisAlignment.start,
                                            children: [
                                              AutoSizeText(
                                                entry.userId,
                                                style: TextStyle(
                                                  color: Colors.black,
                                                  fontSize: 18,
                                                ),
                                                maxLines: 1,
                                                overflow: TextOverflow.ellipsis,
                                              ),
                                              SizedBox(height: 12),
                                              Text(
                                                entry.dataHash,
                                                style: TextStyle(
                                                  color: Colors.black54,
                                                ),
                                              ),
                                              SizedBox(height: 12),
                                              Text(
                                                entry.nonce?.toString() ?? '*',
                                                style: TextStyle(
                                                  color: Colors.black54,
                                                ),
                                              ),
                                            ],
                                          ),
                                        ),
                                      ),
                                    ),
                                    ClippedBlockNumberContainer(
                                      color: Colors.blueAccent,
                                      height: 120,
                                      child: Text(
                                        entry.joinedBlockNumber?.toString() ??
                                            '*',
                                        style: TextStyle(
                                          color: Colors.white,
                                          fontSize: 26,
                                        ),
                                      ),
                                    ),
                                    CornerBanner(
                                      position: Alignment.topLeft,
                                      size: 40,
                                      cornerRadius: 12,
                                      color: Colors.white,
                                      child: Text((index + 1).toString()),
                                    )
                                  ],
                                ),
                              ),
                            );
                          },
                        ),
                      ),
                    ],
                  );
                },
              ),
            ),
            Expanded(
              child: StreamBuilder(
                stream: _settlementBloc.verifierLotteryInfo$,
                builder: (context, snapshot) {
                  if (snapshot.data == null) {
                    return Center(child: CircularProgressIndicator());
                  }

                  var info = snapshot.data!;

                  return StreamBuilder(
                    stream: _ethereumBloc.latestBlockNumber$,
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
                        child: SingleChildScrollView(
                          child: Column(
                            children: [
                              SizedBox(height: 24),
                              Stack(
                                alignment: Alignment.center,
                                children: [
                                  SleekCircularSlider(
                                    min: startBlock,
                                    max: endBlock,
                                    initialValue: currentBlock,
                                    appearance: CircularSliderAppearance(
                                      size: 300,
                                    ),
                                    innerWidget: (_) => SizedBox.shrink(),
                                  ),
                                  if (info.initBlock != null)
                                    BlockCountdown(
                                      blocksLeft:
                                          (endBlock - currentBlock).toInt(),
                                    ),
                                ],
                              ),
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
      },
    );
  }
}
