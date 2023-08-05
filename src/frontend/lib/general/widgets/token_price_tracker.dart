import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../widget_extensions.dart';

class TokenPriceTracker extends StatefulWidget {
  const TokenPriceTracker({super.key});

  @override
  State<TokenPriceTracker> createState() => _TokenPriceTrackerState();
}

class _TokenPriceTrackerState extends StateX<TokenPriceTracker> {
  @override
  Widget buildX(BuildContext context) {
    return Container(
      color: const Color(0xFF242423),
      width: 300,
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
        children: [
          Text(
            '1 ETH: 1800.0 USD',
            style: GoogleFonts.righteous(
              color: Colors.white,
              fontSize: 12,
            ),
          ),
          Text(
            '1 TRU: 0.0 USD',
            style: GoogleFonts.righteous(
              color: Colors.white,
              fontSize: 12,
            ),
          ),
        ],
      ),
    );
  }
}
