import 'dart:math';

import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

import '../../general/models/vm/verifier_lottery_participant_entry_vm.dart';
import '../../general/utils/utils.dart';
import '../../general/widgets/block_countdown.dart';
import '../../general/widgets/clipped_rect.dart';
import '../../general/widgets/corner_banner.dart';
import '../bloc/settlement_actions.dart';
import '../bloc/settlement_bloc.dart';
import '../models/vm/settlement_proposal_vm.dart';
import '../../ethereum/bloc/ethereum_bloc.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';
import '../models/vm/verifier_lottery_info_vm.dart';
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

  late Future<VerifierLotteryInfoVm?> _initialInfoRetrieved;

  OverlayEntry? _overlayEntry;

  @override
  void initState() {
    super.initState();

    _currentUserId = _userBloc.latestCurrentUser?.id;

    _settlementBloc.dispatch(
      GetVerifierLotteryParticipants(
        thingId: widget.proposal.thingId,
        proposalId: widget.proposal.id,
      ),
    );
    _initialInfoRetrieved = _settlementBloc.execute<VerifierLotteryInfoVm>(
      GetVerifierLotteryInfo(
        thingId: widget.proposal.thingId,
        proposalId: widget.proposal.id,
      ),
    );
  }

  @override
  void didUpdateWidget(covariant Lottery oldWidget) {
    super.didUpdateWidget(oldWidget);
    _currentUserId = _userBloc.latestCurrentUser?.id;
    _initialInfoRetrieved = _settlementBloc.execute<VerifierLotteryInfoVm>(
      GetVerifierLotteryInfo(
        thingId: widget.proposal.thingId,
        proposalId: widget.proposal.id,
      ),
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
      future: _initialInfoRetrieved,
      builder: (context, snapshot) {
        if (snapshot.data == null) return const Center(child: CircularProgressIndicator());

        var initialInfo = snapshot.data!;

        // @@TODO: Move from here.
        var horizontalMargin = 40.0;
        var availableWidth = 1400.0 - horizontalMargin * 2;
        var participantCardWidth = 150.0;
        var crossAxisSpacing = 20.0;
        var crossAxisCount = (availableWidth + crossAxisSpacing) ~/ (participantCardWidth + crossAxisSpacing);

        return Container(
          padding: EdgeInsets.symmetric(horizontal: horizontalMargin),
          decoration: const BoxDecoration(
            color: Color(0xFF413C69),
            borderRadius: BorderRadius.only(
              topLeft: Radius.circular(16),
              topRight: Radius.circular(16),
            ),
          ),
          child: Column(
            children: [
              const SizedBox(height: 30),
              StreamBuilder(
                stream: _settlementBloc.verifierLotteryInfo$,
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
                            child: LotteryStepper(
                              proposal: widget.proposal,
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
              const SizedBox(height: 30),
              Expanded(
                child: StreamBuilder(
                  stream: _settlementBloc.verifierLotteryParticipants$,
                  builder: (context, snapshot) {
                    if (snapshot.data == null) return const Center(child: CircularProgressIndicator());

                    var result = snapshot.data!;
                    var orchestratorCommitment = result.orchestratorCommitment;
                    var lotteryClosedEvent = result.lotteryClosedEvent;
                    var participants = result.participants;
                    var claimants = result.claimants;

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
                                  'Orchestrator\'s\n         commitment',
                                  style: GoogleFonts.philosopher(
                                    color: Colors.white,
                                    fontSize: 15,
                                  ),
                                ),
                              ),
                              if (orchestratorCommitment != null)
                                Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    if (lotteryClosedEvent == null)
                                      Text(
                                        'No nonce yet',
                                        style: GoogleFonts.righteous(
                                          color: Colors.black.withOpacity(0.7),
                                          fontSize: 16,
                                        ),
                                      )
                                    else
                                      InkWell(
                                        onTapDown: (details) => showOverlay(
                                          context: context,
                                          onOverlayEntryCreated: (overlayEntry) => _overlayEntry = overlayEntry,
                                          onOverlayEntryRemoveRequested: _removeOverlay,
                                          position: details.globalPosition,
                                          title: 'Close lottery transaction',
                                          urlText: 'See on block explorer...',
                                          url: dotenv.env['BLOCK_EXPLORER_URL'] != null
                                              ? '${dotenv.env['BLOCK_EXPLORER_URL']}/tx/${lotteryClosedEvent.txnHash}'
                                              : null,
                                        ),
                                        child: Row(
                                          children: [
                                            Text(
                                              lotteryClosedEvent.nonce?.toString() ?? 'Lottery failed',
                                              style: GoogleFonts.righteous(
                                                color: Colors.black.withOpacity(0.7),
                                                fontSize: 16,
                                              ),
                                            ),
                                            const SizedBox(width: 6),
                                            const Icon(
                                              Icons.launch,
                                              color: Colors.black,
                                              size: 12,
                                            ),
                                          ],
                                        ),
                                      ),
                                    Padding(
                                      padding: const EdgeInsets.only(left: 24),
                                      child: InkWell(
                                        onTapDown: (details) => showOverlay(
                                          context: context,
                                          onOverlayEntryCreated: (overlayEntry) => _overlayEntry = overlayEntry,
                                          onOverlayEntryRemoveRequested: _removeOverlay,
                                          position: details.globalPosition,
                                          title: 'Initialize lottery transaction',
                                          urlText: 'See on block explorer...',
                                          url: dotenv.env['BLOCK_EXPLORER_URL'] != null
                                              ? '${dotenv.env['BLOCK_EXPLORER_URL']}/tx/${orchestratorCommitment.txnHash}'
                                              : null,
                                        ),
                                        child: Row(
                                          children: [
                                            Text(
                                              orchestratorCommitment.commitmentShort,
                                              style: GoogleFonts.raleway(fontSize: 13),
                                            ),
                                            const SizedBox(width: 4),
                                            const Icon(
                                              Icons.launch,
                                              color: Colors.black,
                                              size: 12,
                                            ),
                                          ],
                                        ),
                                      ),
                                    ),
                                  ],
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
                                onPressed: () => _settlementBloc.dispatch(
                                  GetVerifierLotteryParticipants(
                                    thingId: widget.proposal.thingId,
                                    proposalId: widget.proposal.id,
                                  ),
                                ),
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
                              VerifierLotteryParticipantEntryVm entry;
                              bool isClaimant;
                              if (index < participants.length) {
                                entry = participants[index];
                                isClaimant = false;
                              } else {
                                entry = claimants[index - participants.length];
                                isClaimant = true;
                              }

                              return Card(
                                margin: EdgeInsets.zero,
                                color: entry.userId == _currentUserId
                                    ? isClaimant
                                        ? const Color(0xffb15052)
                                        : const Color(0xff0A6EBD)
                                    : Colors.white,
                                shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(8),
                                ),
                                elevation: 5,
                                child: Stack(
                                  children: [
                                    Column(
                                      children: [
                                        Expanded(
                                          child: Container(
                                            width: double.infinity,
                                            decoration: BoxDecoration(
                                              color: isClaimant ? entry.warmCardColor : entry.coldCardColor,
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
                                                  entry.nonceString,
                                                  style: GoogleFonts.righteous(
                                                    color: Colors.white,
                                                    fontSize: 30,
                                                  ),
                                                ),
                                                const SizedBox(height: 6),
                                                InkWell(
                                                  onTapDown: (details) => showOverlay(
                                                    context: context,
                                                    onOverlayEntryCreated: (overlayEntry) =>
                                                        _overlayEntry = overlayEntry,
                                                    onOverlayEntryRemoveRequested: _removeOverlay,
                                                    position: details.globalPosition,
                                                    title: isClaimant
                                                        ? 'Claim lottery spot transaction'
                                                        : 'Join lottery transaction',
                                                    urlText: 'See on block explorer...',
                                                    url: dotenv.env['BLOCK_EXPLORER_URL'] != null
                                                        ? '${dotenv.env['BLOCK_EXPLORER_URL']}/tx/${entry.txnHash}'
                                                        : null,
                                                  ),
                                                  child: Column(
                                                    children: [
                                                      Text(
                                                        entry.commitment,
                                                        style: GoogleFonts.raleway(
                                                          color: Colors.white,
                                                          fontSize: 17,
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
                                            message: 'User: ${entry.walletAddress}',
                                            child: Text(
                                              entry.walletAddressShort,
                                              style: GoogleFonts.raleway(
                                                color: entry.userId == _currentUserId ? Colors.white : Colors.black,
                                              ),
                                            ),
                                          ),
                                        ),
                                      ],
                                    ),
                                    if (entry.isWinner)
                                      Positioned(
                                        top: 0,
                                        left: 0,
                                        child: CornerBanner(
                                          position: Alignment.topLeft,
                                          color: isClaimant
                                              ? const Color(0xffF6BC66)
                                              : const Color.fromARGB(255, 255, 92, 130),
                                          cornerRadius: 8,
                                          size: 26,
                                          child: const SizedBox.shrink(),
                                        ),
                                      ),
                                  ],
                                ),
                              );
                            },
                            itemCount: participants.length + claimants.length,
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
