import 'dart:math';

import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

import '../../general/widgets/corner_banner.dart';
import '../../general/widgets/block_countdown.dart';
import '../../user/bloc/user_bloc.dart';
import '../models/rvm/verifier_lottery_info_vm.dart';
import 'lottery_stepper.dart';
import '../../ethereum/bloc/ethereum_bloc.dart';
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
  late final _thingBloc = use<ThingBloc>();
  late final _ethereumBloc = use<EthereumBloc>();
  late final _userBloc = use<UserBloc>();

  String? _currentUserId;

  late Future<VerifierLotteryInfoVm?> _initialInfoRetrieved;

  @override
  void initState() {
    super.initState();
    _currentUserId = _userBloc.latestCurrentUser?.id;
    _thingBloc.dispatch(GetVerifierLotteryParticipants(thingId: widget.thing.id));
    _initialInfoRetrieved = _thingBloc.execute<VerifierLotteryInfoVm>(GetVerifierLotteryInfo(thingId: widget.thing.id));
  }

  @override
  void didUpdateWidget(covariant Lottery oldWidget) {
    super.didUpdateWidget(oldWidget);
    _currentUserId = _userBloc.latestCurrentUser?.id;
    _initialInfoRetrieved = _thingBloc.execute<VerifierLotteryInfoVm>(GetVerifierLotteryInfo(thingId: widget.thing.id));
  }

  @override
  Widget buildX(BuildContext context) {
    return FutureBuilder(
      future: _initialInfoRetrieved,
      builder: (context, snapshot) {
        if (snapshot.data == null) return Center(child: CircularProgressIndicator());

        var initialInfo = snapshot.data!;

        var horizontalMargin = 40.0;
        var availableWidth = 1400.0 - horizontalMargin * 2;
        var participantCardWidth = 150.0;
        var crossAxisSpacing = 20.0;
        var crossAxisCount = (availableWidth + crossAxisSpacing) ~/ (participantCardWidth + crossAxisSpacing);

        return Container(
          padding: EdgeInsets.symmetric(horizontal: horizontalMargin),
          decoration: BoxDecoration(
            color: Color(0xFF413C69),
            borderRadius: BorderRadius.only(
              topLeft: Radius.circular(16),
              topRight: Radius.circular(16),
            ),
          ),
          child: Column(
            children: [
              SizedBox(height: 40),
              StreamBuilder(
                stream: _thingBloc.verifierLotteryInfo$,
                initialData: initialInfo,
                builder: (context, snapshot) {
                  var info = snapshot.data!;
                  return StreamBuilder(
                    stream: _ethereumBloc.latestL1BlockNumber$,
                    initialData: _ethereumBloc.latestL1BlockNumber,
                    builder: (context, snapshot) {
                      var latestBlockNumber = snapshot.data!.toDouble();
                      var startBlock = info.initBlock?.abs().toDouble() ?? 0;
                      var endBlock = startBlock + info.durationBlocks;
                      var currentBlock = 0.0;
                      if (info.initBlock != null) {
                        currentBlock = min(latestBlockNumber, endBlock).toDouble();
                      }

                      return Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
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
                              if (info.initBlock != null) BlockCountdown(blocksLeft: (endBlock - currentBlock).toInt()),
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
                          SizedBox(width: 40),
                          SizedBox(
                            width: 600,
                            child: LotteryStepper(
                              thing: widget.thing,
                              info: info,
                              currentBlock: currentBlock.toInt(),
                              endBlock: endBlock.toInt(),
                            ),
                          ),
                        ],
                      );
                    },
                  );
                },
              ),
              SizedBox(height: 30),
              Expanded(
                child: StreamBuilder(
                  stream: _thingBloc.verifierLotteryParticipants$,
                  builder: (context, snapshot) {
                    if (snapshot.data == null) return Center(child: CircularProgressIndicator());

                    var result = snapshot.data!;
                    var orchestratorCommitment = result.orchestratorCommitment;
                    var lotteryClosedEvent = result.lotteryClosedEvent;
                    var participants = result.participants;

                    return Column(
                      children: [
                        Card(
                          margin: const EdgeInsets.symmetric(horizontal: 40),
                          color: Colors.white,
                          elevation: 2,
                          child: Row(
                            children: [
                              Text(
                                'Lottery',
                                style: GoogleFonts.philosopher(fontSize: 24),
                              ),
                              if (orchestratorCommitment != null)
                                Text(
                                  'Orchestrator commitment: ${orchestratorCommitment.commitmentShort}',
                                  style: GoogleFonts.raleway(),
                                ),
                              Spacer(),
                              IconButton(
                                icon: Icon(Icons.refresh),
                                onPressed: () => _thingBloc.dispatch(
                                  GetVerifierLotteryParticipants(thingId: widget.thing.id),
                                ),
                              ),
                            ],
                          ),
                        ),
                        SizedBox(height: 28),
                        Expanded(
                          child: GridView.builder(
                            gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                              crossAxisCount: crossAxisCount,
                              crossAxisSpacing: crossAxisSpacing,
                              mainAxisExtent: 200,
                              mainAxisSpacing: 16,
                            ),
                            itemBuilder: (context, index) {
                              var participant = participants[0];
                              return Card(
                                margin: EdgeInsets.zero,
                                color: Colors.white,
                                elevation: 5,
                                clipBehavior: Clip.antiAlias,
                                child: Stack(
                                  children: [
                                    Column(
                                      children: [
                                        Expanded(
                                          child: Container(
                                            width: double.infinity,
                                            color: Colors.blue,
                                            alignment: Alignment.center,
                                            child: Column(
                                              mainAxisAlignment: MainAxisAlignment.center,
                                              children: [
                                                Text(
                                                  participant.nonceString,
                                                  style: GoogleFonts.righteous(
                                                    color: Colors.white,
                                                    fontSize: 30,
                                                  ),
                                                ),
                                                SizedBox(height: 6),
                                                Text(
                                                  participant.commitment,
                                                  style: GoogleFonts.raleway(
                                                    color: Colors.white,
                                                    fontSize: 20,
                                                  ),
                                                ),
                                              ],
                                            ),
                                          ),
                                        ),
                                        Padding(
                                          padding: const EdgeInsets.symmetric(vertical: 8),
                                          child: Text(
                                            participant.userIdShort,
                                            style: GoogleFonts.philosopher(),
                                          ),
                                        ),
                                      ],
                                    ),
                                    if (participant.userId == _currentUserId)
                                      Positioned(
                                        top: 0,
                                        left: 0,
                                        child: CornerBanner(
                                          position: Alignment.topLeft,
                                          color: Colors.red,
                                          cornerRadius: 4,
                                          size: 26,
                                          child: SizedBox.shrink(),
                                        ),
                                      ),
                                    if (participant.isWinner)
                                      Positioned(
                                        bottom: 0,
                                        right: 0,
                                        child: CornerBanner(
                                          position: Alignment.bottomRight,
                                          color: Colors.green,
                                          cornerRadius: 4,
                                          size: 26,
                                          child: SizedBox.shrink(),
                                        ),
                                      ),
                                  ],
                                ),
                              );
                            },
                            itemCount: 11,
                          ),
                        ),
                        SizedBox(height: 12),
                      ],
                    );
                  },
                ),
              ),
            ],
          ),
        );
      },
    );
  }
}
