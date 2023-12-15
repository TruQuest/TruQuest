import 'dart:math';

import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

import '../../general/utils/utils.dart';
import '../../general/widgets/clipped_rect.dart';
import '../models/vm/validation_poll_info_vm.dart';
import '../../general/widgets/block_countdown.dart';
import '../../ethereum/bloc/ethereum_bloc.dart';
import '../../user/bloc/user_bloc.dart';
import '../bloc/thing_actions.dart';
import '../bloc/thing_bloc.dart';
import '../models/vm/thing_state_vm.dart';
import '../models/vm/thing_vm.dart';
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
  late final _userBloc = use<UserBloc>();

  String? _currentUserId;

  late Future<ValidationPollInfoVm?> _infoRetrieved;

  OverlayEntry? _overlayEntry;

  @override
  void initState() {
    super.initState();
    _currentUserId = _userBloc.latestCurrentUser?.id;
    _thingBloc.dispatch(GetVotes(thingId: widget.thing.id));
    _infoRetrieved = _thingBloc.execute<ValidationPollInfoVm>(
      GetValidationPollInfo(thingId: widget.thing.id),
    );
  }

  @override
  void didUpdateWidget(covariant Poll oldWidget) {
    super.didUpdateWidget(oldWidget);
    _currentUserId = _userBloc.latestCurrentUser?.id;
    _thingBloc.dispatch(GetVotes(thingId: widget.thing.id));
    _infoRetrieved = _thingBloc.execute<ValidationPollInfoVm>(
      GetValidationPollInfo(thingId: widget.thing.id),
    );
  }

  @override
  void dispose() {
    _removeOverlay();
    super.dispose();
  }

  void _removeOverlay() {
    _overlayEntry?.remove();
    _overlayEntry?.dispose();
    _overlayEntry = null;
  }

  @override
  Widget buildX(BuildContext context) {
    return FutureBuilder(
      future: _infoRetrieved,
      builder: (context, snapshot) {
        if (snapshot.data == null) return const Center(child: CircularProgressIndicator());

        var info = snapshot.data!;

        // @@TODO: Move from here.
        var horizontalMargin = 40.0;
        var availableWidth = 1400.0 - horizontalMargin * 2;
        var participantCardWidth = 150.0;
        var crossAxisSpacing = 20.0;
        var crossAxisCount = (availableWidth + crossAxisSpacing) ~/ (participantCardWidth + crossAxisSpacing);

        return Container(
          padding: EdgeInsets.symmetric(horizontal: horizontalMargin),
          decoration: const BoxDecoration(
            color: Color(0xFF32407B),
            borderRadius: BorderRadius.only(
              topLeft: Radius.circular(16),
              topRight: Radius.circular(16),
            ),
          ),
          child: Column(
            children: [
              const SizedBox(height: 30),
              StreamBuilder(
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
                    children: [
                      Stack(
                        alignment: Alignment.center,
                        children: [
                          SleekCircularSlider(
                            min: startBlock,
                            max: endBlock,
                            initialValue: currentBlock,
                            appearance: CircularSliderAppearance(
                              size: 270,
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
                      const SizedBox(width: 80),
                      SizedBox(
                        width: 600,
                        child: PollStepper(
                          thing: widget.thing,
                          info: info,
                          currentBlock: currentBlock.toInt(),
                          endBlock: endBlock.toInt(),
                        ),
                      ),
                    ],
                  );
                },
              ),
              const SizedBox(height: 30),
              Expanded(
                child: StreamBuilder(
                  stream: _thingBloc.votes$,
                  builder: (context, snapshot) {
                    if (snapshot.data == null) return const Center(child: CircularProgressIndicator());

                    var result = snapshot.data!;
                    var votes = result.votes;

                    return Column(
                      children: [
                        Card(
                          margin: const EdgeInsets.symmetric(horizontal: 40),
                          color: Colors.white,
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(8),
                          ),
                          elevation: 2,
                          child: Row(
                            children: [
                              ClippedRect(
                                width: 200,
                                height: 50,
                                color: Colors.blue,
                                fromNarrowToWide: true,
                                narrowSideFraction: 0.4,
                                borderRadius: const BorderRadius.only(
                                  topLeft: Radius.circular(8),
                                  bottomLeft: Radius.circular(8),
                                ),
                                child: Text(
                                  result.thingState == ThingStateVm.verifiersSelectedAndPollInitiated
                                      ? 'Poll in progress'
                                      : 'Poll finalized',
                                  style: GoogleFonts.philosopher(
                                    color: Colors.white,
                                    fontSize: 18,
                                  ),
                                ),
                              ),
                              if (result.voteAggIpfsCid == null)
                                Text(
                                  'Decision: Pending',
                                  style: GoogleFonts.raleway(
                                    fontSize: 15,
                                    fontWeight: FontWeight.bold,
                                  ),
                                )
                              else
                                InkWell(
                                  onTapDown: (details) => showOverlay(
                                    context: context,
                                    onOverlayEntryCreated: (overlayEntry) => _overlayEntry = overlayEntry,
                                    onOverlayEntryRemoveRequested: _removeOverlay,
                                    position: details.globalPosition,
                                    title: 'Validation poll aggregate vote',
                                    urlText: 'See on IPFS...',
                                    url: '${dotenv.env['IPFS_GATEWAY_URL']}/${result.voteAggIpfsCid}',
                                  ),
                                  child: Row(
                                    children: [
                                      Text(
                                        'Decision: ${result.decision}',
                                        style: GoogleFonts.raleway(
                                          fontSize: 15,
                                          fontWeight: FontWeight.bold,
                                        ),
                                      ),
                                      const SizedBox(width: 6),
                                      const Icon(
                                        Icons.launch,
                                        color: Colors.black,
                                        size: 14,
                                      ),
                                    ],
                                  ),
                                ),
                              const Spacer(),
                              SizedBox(
                                width: 170,
                                height: 30,
                                child: TextField(
                                  decoration: InputDecoration(
                                    hintText: 'Search',
                                    hintStyle: GoogleFonts.raleway(
                                      fontSize: 14,
                                    ),
                                    contentPadding: const EdgeInsets.only(left: 12),
                                    suffixIcon: const Icon(Icons.search, size: 20),
                                    border: const OutlineInputBorder(),
                                  ),
                                ),
                              ),
                              const SizedBox(width: 12),
                              IconButton(
                                icon: const Icon(Icons.refresh, size: 20),
                                onPressed: () => _thingBloc.dispatch(GetVotes(thingId: widget.thing.id)),
                              ),
                            ],
                          ),
                        ),
                        const SizedBox(height: 28),
                        Expanded(
                          child: GridView.builder(
                            gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                              crossAxisCount: crossAxisCount,
                              crossAxisSpacing: crossAxisSpacing,
                              mainAxisExtent: 200,
                              mainAxisSpacing: 24,
                            ),
                            itemBuilder: (context, index) {
                              var vote = votes[index];

                              return Card(
                                margin: EdgeInsets.zero,
                                color: vote.userId == _currentUserId ? const Color(0xff0A6EBD) : Colors.white,
                                shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(8),
                                ),
                                elevation: 5,
                                child: Column(
                                  children: [
                                    Expanded(
                                      child: Container(
                                        width: double.infinity,
                                        decoration: BoxDecoration(
                                          color: vote.cardColor,
                                          borderRadius: const BorderRadius.only(
                                            topLeft: Radius.circular(8),
                                            topRight: Radius.circular(8),
                                          ),
                                        ),
                                        alignment: Alignment.center,
                                        child: Column(
                                          mainAxisAlignment: MainAxisAlignment.center,
                                          children: [
                                            Text(
                                              vote.decisionString,
                                              style: GoogleFonts.righteous(
                                                color: Colors.white,
                                                fontSize: 24,
                                              ),
                                            ),
                                            const SizedBox(height: 6),
                                            InkWell(
                                              onTapDown: (details) => showOverlay(
                                                context: context,
                                                onOverlayEntryCreated: (overlayEntry) => _overlayEntry = overlayEntry,
                                                onOverlayEntryRemoveRequested: _removeOverlay,
                                                position: details.globalPosition,
                                                title: 'User\'s vote',
                                                urlText:
                                                    'See on ${vote.blockNumber != null ? 'block explorer' : 'IPFS'}...',
                                                url: vote.blockNumber != null
                                                    ? dotenv.env['BLOCK_EXPLORER_URL'] != null
                                                        ? '${dotenv.env['BLOCK_EXPLORER_URL']}/tx/${vote.txnHash}'
                                                        : null
                                                    : '${dotenv.env['IPFS_GATEWAY_URL']}/${vote.ipfsCid}',
                                              ),
                                              child: Column(
                                                children: [
                                                  Text(
                                                    vote.onOrOffChain,
                                                    style: GoogleFonts.raleway(
                                                      color: Colors.white,
                                                      fontSize: 20,
                                                    ),
                                                  ),
                                                  const SizedBox(height: 6),
                                                  Icon(
                                                    Icons.launch,
                                                    color: Colors.black.withOpacity(0.4),
                                                    size: 18,
                                                  ),
                                                ],
                                              ),
                                            ),
                                          ],
                                        ),
                                      ),
                                    ),
                                    Padding(
                                      padding: const EdgeInsets.symmetric(vertical: 8),
                                      child: Tooltip(
                                        message: 'User: ${vote.walletAddress}',
                                        child: Text(
                                          vote.walletAddressShort,
                                          style: GoogleFonts.raleway(
                                            color: vote.userId == _currentUserId ? Colors.white : Colors.black,
                                          ),
                                        ),
                                      ),
                                    ),
                                  ],
                                ),
                              );
                            },
                            itemCount: votes.length,
                          ),
                        ),
                        const SizedBox(height: 12),
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
