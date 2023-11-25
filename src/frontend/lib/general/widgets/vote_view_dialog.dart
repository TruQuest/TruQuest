import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../models/vm/decision_vm.dart';
import '../models/vm/vote_vm.dart';

class VoteViewDialog extends StatelessWidget {
  final VoteVm vote;

  const VoteViewDialog({super.key, required this.vote});

  @override
  Widget build(BuildContext context) {
    return SimpleDialog(
      backgroundColor: const Color(0xFF242423),
      title: Text(
        'Vote',
        style: GoogleFonts.philosopher(
          color: Colors.white,
        ),
      ),
      children: [
        SizedBox(
          width: 400,
          child: Column(
            children: [
              Text(
                vote.userId,
                style: GoogleFonts.raleway(
                  color: Colors.white,
                ),
              ),
              Text(
                vote.decision!.getString(),
                style: GoogleFonts.raleway(
                  color: Colors.white,
                ),
              ),
              Text(
                vote.ipfsCid ?? vote.txnHash!,
                style: GoogleFonts.raleway(
                  color: Colors.white,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
