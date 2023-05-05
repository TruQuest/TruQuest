import 'dart:math';

import 'package:flutter/material.dart';
import 'package:auto_size_text/auto_size_text.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';
import 'package:timelines/timelines.dart';

import 'clipped_block_number_container.dart';
import 'swipe_button.dart';
import '../../general/utils/utils.dart';
import '../../ethereum/bloc/ethereum_bloc.dart';
import '../../subject/widgets/corner_banner.dart';
import '../../user/bloc/user_bloc.dart';
import '../bloc/thing_actions.dart';
import '../bloc/thing_bloc.dart';
import '../models/rvm/thing_vm.dart';
import '../../widget_extensions.dart';

class Lottery extends StatefulWidget {
  final ThingVm thing;

  const Lottery({super.key, required this.thing});

  @override
  State<Lottery> createState() => _LotteryState();
}

class _LotteryState extends StateX<Lottery> {
  late final _userBloc = use<UserBloc>();
  late final _thingBloc = use<ThingBloc>();
  late final _ethereumBloc = use<EthereumBloc>();

  final _counterAppearance = CircularSliderAppearance(
    customWidths: CustomSliderWidths(
      trackWidth: 4,
      progressBarWidth: 30,
      shadowWidth: 60,
    ),
    customColors: CustomSliderColors(
      dotColor: Colors.white.withOpacity(0.1),
      trackColor: Color(0xffF9EBE0).withOpacity(0.5),
      progressBarColors: [
        Color(0xffA586EE).withOpacity(0.3),
        Color(0xffF9D3D2).withOpacity(0.3),
        Color(0xffBF79C2).withOpacity(0.3),
      ],
      shadowColor: Color(0xff7F5ED9),
      shadowMaxOpacity: 0.05,
    ),
    startAngle: 180,
    angleRange: 360,
    size: 220.0,
  );

  @override
  void initState() {
    super.initState();
    _thingBloc.dispatch(
      GetVerifierLotteryParticipants(thingId: widget.thing.id),
    );
  }

  @override
  Widget build(BuildContext context) {
    return StreamBuilder(
      stream: _userBloc.currentUser$,
      builder: (context, snapshot) {
        var user = snapshot.data?.user;
        _thingBloc.dispatch(GetVerifierLotteryInfo(thingId: widget.thing.id));

        return Row(
          children: [
            Expanded(
              child: StreamBuilder(
                stream: _thingBloc.verifierLotteryInfo$,
                builder: (context, snapshot) {
                  if (snapshot.data == null) {
                    return Center(child: CircularProgressIndicator());
                  }

                  var info = snapshot.data!;

                  return StreamBuilder(
                    stream: _ethereumBloc.latestBlockNumber$,
                    builder: (context, snapshot) {
                      var latestBlockNumber = snapshot.data?.toDouble() ?? 0;
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
                                  StreamBuilder(
                                    stream: Stream.fromFutures(
                                      [
                                        Future.delayed(
                                          Duration(seconds: 1),
                                          () => true,
                                        ),
                                        Future.delayed(
                                          Duration(seconds: 5),
                                          () => false,
                                        ),
                                      ],
                                    ),
                                    initialData: false,
                                    builder: (context, snapshot) {
                                      return SleekCircularSlider(
                                        min: 0,
                                        max: 360,
                                        initialValue: !snapshot.data! ? 0 : 360,
                                        appearance: _counterAppearance,
                                        innerWidget: (value) {
                                          return Transform.rotate(
                                            angle: degreesToRadians(value),
                                            child: Align(
                                              alignment: Alignment.center,
                                              child: Container(
                                                width: value / 4,
                                                height: value / 4,
                                                decoration: BoxDecoration(
                                                  border: Border.all(
                                                    color: Colors.white,
                                                  ),
                                                  borderRadius:
                                                      BorderRadius.circular(
                                                    4,
                                                  ),
                                                ),
                                                alignment: Alignment.center,
                                                child: FittedBox(
                                                  child: Text(
                                                    '${endBlock - currentBlock}\nleft',
                                                    textAlign: TextAlign.center,
                                                    style: TextStyle(
                                                      color: Colors.white,
                                                    ),
                                                  ),
                                                ),
                                              ),
                                            ),
                                          );
                                        },
                                      );
                                    },
                                  ),
                              ],
                            ),
                            Transform.translate(
                              offset: Offset(-20, 0),
                              child: SizedBox(
                                width: 340,
                                child: FixedTimeline.tileBuilder(
                                  mainAxisSize: MainAxisSize.min,
                                  theme: TimelineThemeData(nodePosition: 0),
                                  builder: TimelineTileBuilder.connected(
                                    itemCount: 2,
                                    contentsAlign: ContentsAlign.basic,
                                    indicatorBuilder: (context, index) =>
                                        CircleAvatar(
                                      radius: 15,
                                      child: Text((index + 1).toString()),
                                    ),
                                    connectorBuilder: (context, index, type) =>
                                        SolidLineConnector(),
                                    contentsBuilder: (context, index) {
                                      if (index == 0) {
                                        return Padding(
                                          padding: const EdgeInsets.fromLTRB(
                                            12,
                                            4,
                                            0,
                                            4,
                                          ),
                                          child: SwipeButton(
                                            text: 'Commit to lottery',
                                            onCompleteSwipe: () async {
                                              await Future.delayed(
                                                Duration(seconds: 2),
                                              );
                                              return true;
                                            },
                                          ),
                                        );
                                      }

                                      return Padding(
                                        padding: const EdgeInsets.fromLTRB(
                                          12,
                                          4,
                                          0,
                                          4,
                                        ),
                                        child: SwipeButton(
                                          text: 'Join lottery',
                                          onCompleteSwipe: () async {
                                            await Future.delayed(
                                              Duration(seconds: 2),
                                            );
                                            return false;
                                          },
                                        ),
                                      );
                                    },
                                  ),
                                ),
                              ),
                            ),
                          ],
                        ),
                      );
                    },
                  );
                },
              ),
            ),
            Expanded(
              child: StreamBuilder(
                stream: _thingBloc.verifierLotteryParticipants$,
                builder: (context, snapshot) {
                  if (snapshot.data == null) {
                    return Center(child: CircularProgressIndicator());
                  }

                  var entries = snapshot.data!.entries;

                  return ListView.builder(
                    itemCount: entries.length,
                    itemBuilder: (context, index) {
                      var entry = entries[index];
                      return Padding(
                        padding: const EdgeInsets.symmetric(vertical: 8),
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
                                    mainAxisAlignment: MainAxisAlignment.center,
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
                              child: Text(
                                entry.joinedBlockNumber?.toString() ?? '*',
                                style: TextStyle(
                                  color: Colors.white,
                                  fontSize: 30,
                                ),
                              ),
                            ),
                            CornerBanner(
                              position: Alignment.topLeft,
                              size: 40,
                              cornerRadius: 12,
                              color: Colors.white,
                              icon: Icons.add,
                              iconSize: 14,
                            )
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
