import 'dart:math';

import 'package:flutter/material.dart';
import 'package:auto_size_text/auto_size_text.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

import '../../general/widgets/clipped_block_number_container.dart';
import '../../general/widgets/corner_banner.dart';
import '../../general/widgets/block_countdown.dart';
import '../../ethereum/bloc/ethereum_bloc.dart';
import '../bloc/thing_actions.dart';
import '../bloc/thing_bloc.dart';
import '../models/rvm/thing_vm.dart';
import '../../widget_extensions.dart';
import 'poll_stepper.dart';

class Poll extends StatefulWidget {
  final ThingVm thing;

  const Poll({super.key, required this.thing});

  @override
  State<Poll> createState() => _PollState();
}

class _PollState extends StateX<Poll> {
  late final _thingBloc = use<ThingBloc>();
  late final _ethereumBloc = use<EthereumBloc>();

  late GetAcceptancePollInfo _getInfoAction;

  @override
  void initState() {
    super.initState();
    _thingBloc.dispatch(GetVerifiers(thingId: widget.thing.id));
    _getInfoAction = GetAcceptancePollInfo(thingId: widget.thing.id);
    _thingBloc.dispatch(_getInfoAction);
  }

  @override
  void didUpdateWidget(covariant Poll oldWidget) {
    super.didUpdateWidget(oldWidget);
    _getInfoAction = GetAcceptancePollInfo(thingId: widget.thing.id);
    _thingBloc.dispatch(_getInfoAction);
  }

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Expanded(
          child: StreamBuilder(
            stream: _thingBloc.verifiers$,
            builder: (context, snapshot) {
              if (snapshot.data == null) {
                return Center(child: CircularProgressIndicator());
              }

              var verifiers = snapshot.data!.verifiers;

              return Column(
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
                      'Participants',
                      style: GoogleFonts.philosopher(
                        color: Colors.white,
                        fontSize: 24,
                      ),
                    ),
                  ),
                  Padding(
                    padding: const EdgeInsets.only(top: 22),
                    child: Stack(
                      children: [
                        Card(
                          margin: EdgeInsets.zero,
                          color: Colors.white,
                          elevation: 25,
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(12),
                          ),
                          shadowColor: Colors.white30,
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
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  AutoSizeText(
                                    'User Id',
                                    style: GoogleFonts.philosopher(
                                      color: Colors.black,
                                      fontSize: 18,
                                    ),
                                    maxLines: 1,
                                    overflow: TextOverflow.ellipsis,
                                  ),
                                  SizedBox(height: 6),
                                  Text(
                                    'Username',
                                    style: GoogleFonts.raleway(
                                      color: Colors.black87,
                                    ),
                                  ),
                                  SizedBox(height: 6),
                                  Text(
                                    'On-/Off-Chain',
                                    style: GoogleFonts.raleway(
                                      color: Colors.black87,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ),
                        ),
                        ClippedBlockNumberContainer(
                          color: Color(0xFF4A47A3),
                          height: 80,
                          child: Text(
                            'Block/Time',
                            style: GoogleFonts.righteous(
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
                        ),
                        Positioned(
                          top: 0,
                          bottom: 0,
                          right: 0,
                          child: Center(
                            child: IconButton(
                              icon: Icon(Icons.refresh),
                              onPressed: () => _thingBloc.dispatch(
                                GetVerifiers(thingId: widget.thing.id),
                              ),
                            ),
                          ),
                        ),
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
                                  shadowColor: Colors.white30,
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
                                            style: GoogleFonts.philosopher(
                                              color: Colors.black,
                                              fontSize: 18,
                                            ),
                                            maxLines: 1,
                                            overflow: TextOverflow.ellipsis,
                                          ),
                                          SizedBox(height: 12),
                                          Text(
                                            verifier.username,
                                            style: GoogleFonts.raleway(
                                              color: Colors.black87,
                                            ),
                                          ),
                                          SizedBox(height: 12),
                                          Text(
                                            verifier.vote == null
                                                ? 'No vote'
                                                : verifier.vote!.blockNumber !=
                                                        null
                                                    ? 'On-chain'
                                                    : 'Off-chain',
                                            style: GoogleFonts.raleway(
                                              color: Colors.black87,
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
                                    style: GoogleFonts.righteous(
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
                                  child: Text(
                                    (index + 1).toString(),
                                    style: GoogleFonts.righteous(),
                                  ),
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
            future: _getInfoAction.result,
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
                          thing: widget.thing,
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
