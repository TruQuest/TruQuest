import 'dart:math';

import 'package:flutter/material.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

import '../../ethereum/bloc/ethereum_bloc.dart';
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

  double _degreeToRadians(double degrees) => (pi / 180) * degrees;

  @override
  Widget build(BuildContext context) {
    return StreamBuilder(
      stream: _userBloc.currentUser$,
      builder: (context, _) {
        _thingBloc.dispatch(GetThingLotteryInfo(thingId: widget.thing.id));

        return Column(
          children: [
            Expanded(
              child: StreamBuilder(
                stream: _thingBloc.thingLotteryInfo$,
                builder: (context, snapshot) {
                  if (snapshot.data == null) {
                    return Center(
                      child: CircularProgressIndicator(),
                    );
                  }

                  var info = snapshot.data!;

                  return Row(
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: [
                      Expanded(
                        child: StreamBuilder(
                          stream: _ethereumBloc.latestBlockNumber$,
                          builder: (context, snapshot) {
                            var latestBlockNumber =
                                snapshot.data?.toDouble() ?? 0.0;
                            var startBlock = info.initBlock.toDouble();
                            var endBlock = startBlock + info.durationBlocks;
                            var currentBlock =
                                max(latestBlockNumber, info.latestBlockNumber)
                                    .toDouble();

                            return Center(
                              child: Stack(
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
                                            angle: _degreeToRadians(value),
                                            child: Align(
                                              alignment: Alignment.center,
                                              child: Container(
                                                width: value / 4,
                                                height: value / 4,
                                                decoration: BoxDecoration(
                                                  gradient: LinearGradient(
                                                    colors: [
                                                      Color(0xffF9D3D2)
                                                          .withOpacity(
                                                              value / 360),
                                                      Color(0xffBF79C2)
                                                          .withOpacity(
                                                              value / 360),
                                                    ],
                                                    begin: Alignment.bottomLeft,
                                                    end: Alignment.topRight,
                                                    tileMode: TileMode.clamp,
                                                  ),
                                                ),
                                                alignment: Alignment.center,
                                                child: FittedBox(
                                                  child: Text(
                                                    '${(endBlock - currentBlock)}\nBlocks\nRemaining',
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
                            );
                          },
                        ),
                      ),
                      Expanded(
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            OutlinedButton(
                              child: Text('Commit to lottery'),
                              onPressed: !info.alreadyPreJoined
                                  ? () {
                                      _thingBloc.dispatch(
                                        PreJoinLottery(
                                          thingId: widget.thing.id,
                                        ),
                                      );
                                    }
                                  : null,
                            ),
                            SizedBox(height: 12),
                            OutlinedButton(
                              child: Text('Join lottery'),
                              onPressed: info.alreadyPreJoined &&
                                      !info.alreadyJoined
                                  ? () {
                                      _thingBloc.dispatch(
                                        JoinLottery(thingId: widget.thing.id),
                                      );
                                    }
                                  : null,
                            ),
                          ],
                        ),
                      ),
                    ],
                  );
                },
              ),
            ),
            Expanded(
              child: Center(child: Text('Participants')),
            ),
          ],
        );
      },
    );
  }
}
