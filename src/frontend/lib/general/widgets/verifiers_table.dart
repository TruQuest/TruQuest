import 'package:flutter/material.dart';
import 'package:auto_size_text/auto_size_text.dart';
import 'package:google_fonts/google_fonts.dart';

import '../models/rvm/verifier_vm.dart';
import 'clipped_block_number_container.dart';
import 'corner_banner.dart';

class VerifiersTable extends StatelessWidget {
  final List<VerifierVm> verifiers;
  final String? currentUserId;
  final VoidCallback onRefresh;

  const VerifiersTable({
    super.key,
    required this.verifiers,
    required this.currentUserId,
    required this.onRefresh,
  });

  @override
  Widget build(BuildContext context) {
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
                    onPressed: onRefresh,
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
                              mainAxisAlignment: MainAxisAlignment.center,
                              crossAxisAlignment: CrossAxisAlignment.start,
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
                                      : verifier.vote!.blockNumber != null
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
                      Tooltip(
                        message: verifier.verifierId == currentUserId
                            ? 'You'
                            : 'Verifier',
                        child: ClippedBlockNumberContainer(
                          color: verifier.verifierId == currentUserId
                              ? Color(0xFFE06469)
                              : Colors.blueAccent,
                          height: 120,
                          child: Text(
                            verifier.castedVoteAt,
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
