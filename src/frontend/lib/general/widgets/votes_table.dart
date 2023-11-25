import 'package:flutter/material.dart';
import 'package:auto_size_text/auto_size_text.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../thing/models/vm/get_votes_rvm.dart';
import 'clipped_block_number_container.dart';
import 'corner_banner.dart';
import 'vote_agg_view_dialog.dart';
import 'vote_view_dialog.dart';

class VotesTable extends StatelessWidget {
  final GetVotesRvm result;
  final String? currentUserId;
  final VoidCallback onRefresh;

  const VotesTable({
    super.key,
    required this.result,
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
                    height: 60,
                    child: Row(
                      children: [
                        Expanded(
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
                                'On-/OffChain',
                                style: GoogleFonts.raleway(
                                  color: Colors.black87,
                                ),
                              ),
                            ],
                          ),
                        ),
                        const SizedBox(width: 16),
                        if (result.voteAggIpfsCid != null)
                          Tooltip(
                            message: 'Click to show final votes',
                            child: InkWell(
                              onTap: () => showDialog(
                                context: context,
                                builder: (_) => VoteAggViewDialog(result: result),
                              ),
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
                          ),
                      ],
                    ),
                  ),
                ),
              ),
              ClippedBlockNumberContainer(
                color: const Color(0xFF4A47A3),
                height: 60,
                child: Text(
                  'Block/Time',
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
                  // @@TODO: Loading animation.
                  child: IconButton(
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
            itemCount: result.votes.length,
            itemBuilder: (context, index) {
              var vote = result.votes[index];
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
                            height: 80,
                            child: Row(
                              children: [
                                Expanded(
                                  child: Column(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: [
                                      AutoSizeText(
                                        vote.userId,
                                        style: GoogleFonts.philosopher(
                                          color: Colors.black,
                                          fontSize: 18,
                                        ),
                                        maxLines: 1,
                                        overflow: TextOverflow.ellipsis,
                                      ),
                                      const SizedBox(height: 12),
                                      Text(
                                        vote.castedAtMs != null ? 'Offchain' : 'Onchain',
                                        style: GoogleFonts.raleway(
                                          color: Colors.black87,
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                                const SizedBox(width: 16),
                                if (vote.decision != null)
                                  Tooltip(
                                    message: 'Click to show vote',
                                    child: InkWell(
                                      onTap: () => showDialog(
                                        context: context,
                                        builder: (_) => VoteViewDialog(vote: vote),
                                      ),
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
                                  ),
                              ],
                            ),
                          ),
                        ),
                      ),
                      Tooltip(
                        message: vote.userId == currentUserId ? 'You' : 'Verifier',
                        child: ClippedBlockNumberContainer(
                          color: vote.userId == currentUserId ? const Color(0xFFE06469) : Colors.blueAccent,
                          height: 80,
                          child: Text(
                            vote.castedVoteAt,
                            style: GoogleFonts.righteous(
                              color: Colors.white,
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
