import 'dart:math';

import 'package:flutter/material.dart';
import 'package:auto_size_text/auto_size_text.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

import '../../general/widgets/block_countdown.dart';
import '../../general/widgets/clipped_block_number_container.dart';
import '../../general/widgets/corner_banner.dart';
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

  @override
  void initState() {
    super.initState();
    _settlementBloc.dispatch(GetVerifiers(proposalId: widget.proposal.id));
  }

  @override
  Widget build(BuildContext context) {
    return StreamBuilder(
      stream: _userBloc.currentUser$,
      builder: (context, snapshot) {
        var user = snapshot.data?.user;
        var action = GetAssessmentPollInfo(
          thingId: widget.proposal.thingId,
          proposalId: widget.proposal.id,
        );
        _settlementBloc.dispatch(action);

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
                                        'Username',
                                        style: TextStyle(
                                          color: Colors.black54,
                                        ),
                                      ),
                                      SizedBox(height: 8),
                                      Text(
                                        'On-/Off-Chain',
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
                                'Block/Time',
                                style: TextStyle(
                                  color: Colors.white,
                                  fontSize: 16,
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
                          itemCount: verifiers.length,
                          itemBuilder: (context, index) {
                            var verifier = verifiers[index];
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
                                                verifier.verifierId,
                                                style: TextStyle(
                                                  color: Colors.black,
                                                  fontSize: 18,
                                                ),
                                                maxLines: 1,
                                                overflow: TextOverflow.ellipsis,
                                              ),
                                              SizedBox(height: 12),
                                              Text(
                                                verifier.username,
                                                style: TextStyle(
                                                  color: Colors.black54,
                                                ),
                                              ),
                                              SizedBox(height: 12),
                                              Text(
                                                verifier.vote == null
                                                    ? 'No vote'
                                                    : verifier.vote!
                                                                .blockNumber !=
                                                            null
                                                        ? 'On-chain'
                                                        : 'Off-chain',
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
                                        verifier.castedVoteAt,
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
              child: FutureBuilder(
                future: action.result,
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
      },
    );
  }
}
