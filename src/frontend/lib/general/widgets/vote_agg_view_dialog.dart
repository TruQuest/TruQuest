import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../thing/models/vm/get_votes_rvm.dart';

class VoteAggViewDialog extends StatelessWidget {
  final GetVotesRvm result;

  const VoteAggViewDialog({super.key, required this.result});

  @override
  Widget build(BuildContext context) {
    return SimpleDialog(
      backgroundColor: const Color(0xFF242423),
      title: Text(
        'Agg',
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
                result.voteAggIpfsCid!,
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
