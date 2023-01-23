import 'dart:math';

import 'package:flutter/material.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

import 'vote_dialog.dart';
import '../models/rvm/thing_vm.dart';
import '../../ethereum/bloc/ethereum_bloc.dart';
import '../bloc/thing_actions.dart';
import '../bloc/thing_bloc.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';

class Poll extends StatelessWidgetX {
  late final _userBloc = use<UserBloc>();
  late final _thingBloc = use<ThingBloc>();
  late final _ethereumBloc = use<EthereumBloc>();

  final ThingVm thing;

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

  Poll({super.key, required this.thing});

  double _degreesToRadians(double degrees) => (pi / 180) * degrees;

  @override
  Widget buildX(BuildContext context) {
    return Column(
      children: [
        Expanded(
          child: StreamBuilder(
            stream: _userBloc.currentUser$,
            builder: (context, _) {
              var action = GetAcceptancePollInfo(thingId: thing.id);
              _thingBloc.dispatch(action);

              return FutureBuilder(
                future: action.result,
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

                      return Row(
                        children: [
                          Expanded(
                            child: Center(
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
                                          initialValue:
                                              !snapshot.data! ? 0 : 360,
                                          appearance: _counterAppearance,
                                          innerWidget: (value) {
                                            return Transform.rotate(
                                              angle: _degreesToRadians(value),
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
                                                      begin:
                                                          Alignment.bottomLeft,
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
                            ),
                          ),
                          Expanded(
                            child: Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                OutlinedButton(
                                  child: Text('Vote off-chain'),
                                  onPressed: info.initBlock != null &&
                                          info.isDesignatedVerifier != null &&
                                          currentBlock < endBlock &&
                                          info.isDesignatedVerifier!
                                      ? () {
                                          showDialog(
                                            context: context,
                                            builder: (_) => VoteDialog(
                                              onVote: (decision, reason) {
                                                _thingBloc.dispatch(
                                                  CastVoteOffChain(
                                                    thingId: thing.id,
                                                    decision: decision,
                                                    reason: reason,
                                                  ),
                                                );
                                              },
                                            ),
                                          );
                                        }
                                      : null,
                                ),
                                SizedBox(height: 12),
                                OutlinedButton(
                                  child: Text('Vote on-chain'),
                                  onPressed: info.initBlock != null &&
                                          info.isDesignatedVerifier != null &&
                                          currentBlock < endBlock &&
                                          info.isDesignatedVerifier!
                                      ? () {
                                          showDialog(
                                            context: context,
                                            builder: (_) => VoteDialog(
                                              onVote: (decision, reason) {
                                                _thingBloc.dispatch(
                                                  CastVoteOnChain(
                                                    thingId: thing.id,
                                                    decision: decision,
                                                    reason: reason,
                                                  ),
                                                );
                                              },
                                            ),
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
                  );
                },
              );
            },
          ),
        ),
        Expanded(child: Center(child: Text('Verifiers'))),
      ],
    );
  }
}
