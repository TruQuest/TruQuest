import 'package:flutter/material.dart';
import 'package:animated_text_kit/animated_text_kit.dart';
import 'package:google_fonts/google_fonts.dart';

class MainPageBanner extends StatelessWidget {
  const MainPageBanner({super.key});

  @override
  Widget build(BuildContext context) {
    return Column(
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
        const SizedBox(height: 24),
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
                text: 'Do they fulfill them? Are they worthy of your trust?',
                style: GoogleFonts.raleway(
                  color: Colors.white,
                  fontSize: 20,
                  height: 1.6,
                ),
              ),
            ],
          ),
        ),
        const Spacer(),
        Text(
          'It\'s a neverending quest for truth.',
          style: GoogleFonts.raleway(
            color: Colors.white,
            fontSize: 18,
            height: 3,
          ),
        ),
      ],
    );
  }
}
