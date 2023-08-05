import 'package:flutter/material.dart';
import 'package:animated_text_kit/animated_text_kit.dart';
import 'package:google_fonts/google_fonts.dart';

import 'token_price_tracker.dart';
import '../../widget_extensions.dart';

class WalletInfoCard extends StatefulWidget {
  const WalletInfoCard({super.key});

  @override
  State<WalletInfoCard> createState() => _WalletInfoCardState();
}

class _WalletInfoCardState extends StateX<WalletInfoCard> {
  @override
  Widget buildX(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.end,
      children: [
        TokenPriceTracker(),
        Container(
          color: const Color(0xFF242423),
          height: 284,
          padding: const EdgeInsets.fromLTRB(24, 20, 24, 20),
          child: Row(
            children: [
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Container(
                    color: Colors.black,
                    width: 605,
                    padding: const EdgeInsets.all(8),
                    child: DefaultTextStyle(
                      style: GoogleFonts.righteous(
                        fontSize: 22,
                        color: Colors.white,
                      ),
                      child: Row(
                        children: [
                          const Text('> '),
                          AnimatedTextKit(
                            repeatForever: true,
                            pause: const Duration(seconds: 2),
                            animatedTexts: [
                              TypewriterAnimatedText(
                                'Basically, a promise tracker.',
                                speed: const Duration(milliseconds: 70),
                              ),
                              TypewriterAnimatedText(
                                'People have short memories. Blockchain never forgets.',
                                speed: const Duration(milliseconds: 70),
                              ),
                              TypewriterAnimatedText(
                                'if (promiseKept) reputation++;',
                                speed: const Duration(milliseconds: 70),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                  ),
                  SizedBox(height: 24),
                  RichText(
                    text: TextSpan(
                      children: [
                        TextSpan(
                          text: 'Keep track of people and companies\' promises.\n',
                          style: GoogleFonts.righteous(
                            color: Colors.white,
                            fontSize: 24,
                            height: 1.6,
                          ),
                        ),
                        TextSpan(
                          text: 'Do they fulfill them? Are they worth your trust?',
                          style: GoogleFonts.raleway(
                            color: Colors.white,
                            fontSize: 20,
                            height: 1.6,
                          ),
                        ),
                      ],
                    ),
                  ),
                  Spacer(),
                  Text(
                    'It\'s a neverending quest for truth.',
                    style: GoogleFonts.raleway(
                      color: Colors.white,
                      fontSize: 18,
                      height: 3,
                    ),
                  ),
                ],
              ),
              Spacer(),
              Column(
                children: [
                  SizedBox(height: 16),
                  Text(
                    'Deposit',
                    style: GoogleFonts.philosopher(
                      color: Colors.white70,
                      fontSize: 20,
                    ),
                  ),
                  SizedBox(height: 14),
                  Text(
                    '500/0 TRU',
                    style: GoogleFonts.righteous(
                      color: Colors.white,
                      fontSize: 14,
                    ),
                  ),
                ],
              ),
              SizedBox(width: 80),
              Column(
                children: [
                  SizedBox(height: 16),
                  Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(
                        'Wallet',
                        style: GoogleFonts.philosopher(
                          color: Colors.white70,
                          fontSize: 20,
                        ),
                      ),
                      SizedBox(width: 6),
                      Icon(
                        Icons.check_box_outlined,
                        size: 16,
                        color: Colors.white,
                      ),
                    ],
                  ),
                  SizedBox(height: 8),
                  Text(
                    '0xcccc..fff',
                    style: GoogleFonts.raleway(
                      color: Colors.white,
                      fontSize: 21,
                    ),
                  ),
                  Container(
                    width: 120,
                    height: 44,
                    alignment: Alignment(0, -0.35),
                    child: Divider(color: Colors.white60),
                  ),
                  Text(
                    '0.05 ETH',
                    style: GoogleFonts.righteous(
                      color: Colors.white,
                      fontSize: 14,
                    ),
                  ),
                  SizedBox(height: 12),
                  Text(
                    '1500 TRU',
                    style: GoogleFonts.righteous(
                      color: Colors.white,
                      fontSize: 14,
                    ),
                  ),
                ],
              ),
              SizedBox(width: 80),
              Card(
                color: Colors.white,
                elevation: 5,
                shadowColor: Colors.white,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(8),
                ),
                clipBehavior: Clip.antiAlias,
                child: Column(
                  children: [
                    Image.asset(
                      'assets/images/avatar.png',
                      width: 180,
                      height: 200,
                      fit: BoxFit.cover,
                    ),
                    Text(
                      '0xaaaa..bbb',
                      style: GoogleFonts.raleway(
                        color: Colors.black,
                        fontSize: 16,
                      ),
                    ),
                    SizedBox(height: 12),
                  ],
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
