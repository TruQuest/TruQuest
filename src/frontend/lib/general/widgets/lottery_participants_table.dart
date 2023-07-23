import 'package:flutter/material.dart';
import 'package:auto_size_text/auto_size_text.dart';
import 'package:google_fonts/google_fonts.dart';

import '../models/rvm/verifier_lottery_participant_entry_vm.dart';
import 'clipped_block_number_container.dart';
import 'corner_banner.dart';

class LotteryParticipantsTable extends StatelessWidget {
  final List<VerifierLotteryParticipantEntryVm> entries;
  final String? currentUserId;
  final VoidCallback onRefresh;

  const LotteryParticipantsTable({
    super.key,
    required this.entries,
    required this.currentUserId,
    required this.onRefresh,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
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
                  padding: const EdgeInsets.only(left: 150),
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
                        const SizedBox(height: 6),
                        Text(
                          'Commitment',
                          style: GoogleFonts.raleway(
                            color: Colors.black87,
                          ),
                        ),
                        const SizedBox(height: 6),
                        Text(
                          'Nonce',
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
                color: const Color(0xFF4A47A3),
                height: 80,
                child: Text(
                  'Block',
                  style: GoogleFonts.righteous(
                    color: Colors.white,
                    fontSize: 20,
                  ),
                ),
              ),
              const CornerBanner(
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
                    // @@TODO: Loading animation.
                    icon: const Icon(Icons.refresh),
                    onPressed: onRefresh,
                  ),
                ),
              ),
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
                        shadowColor: Colors.white30,
                        child: Padding(
                          padding: const EdgeInsets.only(left: 150),
                          child: SizedBox(
                            width: 350,
                            height: 120,
                            child: Row(
                              children: [
                                Expanded(
                                  child: Column(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    crossAxisAlignment:
                                        CrossAxisAlignment.start,
                                    children: [
                                      AutoSizeText(
                                        entry.userId,
                                        style: GoogleFonts.philosopher(
                                          color: Colors.black,
                                          fontSize: 18,
                                        ),
                                        maxLines: 1,
                                        overflow: TextOverflow.ellipsis,
                                      ),
                                      const SizedBox(height: 12),
                                      Text(
                                        entry.userData!.substring(0, 20),
                                        style: GoogleFonts.raleway(
                                          color: Colors.black87,
                                        ),
                                      ),
                                      const SizedBox(height: 12),
                                      Text(
                                        entry.nonce?.toString() ?? '*',
                                        style: GoogleFonts.raleway(
                                          color: Colors.black87,
                                          fontSize:
                                              entry.nonce != null ? 16 : 30,
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                                const SizedBox(width: 16),
                                if (entry.isWinner)
                                  Tooltip(
                                    message: 'Winner',
                                    child: Container(
                                      width: 20,
                                      height: double.infinity,
                                      decoration: BoxDecoration(
                                        color: Colors.amber[200],
                                        borderRadius: const BorderRadius.only(
                                          topRight: Radius.circular(12),
                                          bottomRight: Radius.circular(12),
                                        ),
                                      ),
                                    ),
                                  ),
                              ],
                            ),
                          ),
                        ),
                      ),
                      Tooltip(
                        message: entry.isOrchestrator
                            ? 'Orchestrator'
                            : entry.userId == currentUserId
                                ? 'You'
                                : 'Participant',
                        child: ClippedBlockNumberContainer(
                          color: entry.isOrchestrator
                              ? const Color(0x33242423)
                              : entry.userId == currentUserId
                                  ? const Color(0xFFE06469)
                                  : Colors.blueAccent,
                          height: 120,
                          child: Text(
                            entry.joinedBlockNumber.toString(),
                            style: GoogleFonts.righteous(
                              color: entry.isOrchestrator
                                  ? const Color(0xAA242423)
                                  : Colors.white,
                              fontSize: 26,
                            ),
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
  }
}
