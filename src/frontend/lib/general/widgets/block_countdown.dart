import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

import '../../general/utils/utils.dart';

class BlockCountdown extends StatefulWidget {
  final int blocksLeft;

  const BlockCountdown({super.key, required this.blocksLeft});

  @override
  State<BlockCountdown> createState() => _BlockCountdownState();
}

class _BlockCountdownState extends State<BlockCountdown> {
  final _counterAppearance = CircularSliderAppearance(
    customWidths: CustomSliderWidths(
      trackWidth: 4,
      progressBarWidth: 30,
      shadowWidth: 60,
    ),
    customColors: CustomSliderColors(
      dotColor: Colors.transparent,
      trackColor: const Color(0xffF9EBE0).withOpacity(0.5),
      progressBarColors: const [
        Color(0xffA586EE),
        Color(0xffF9D3D2),
        Color(0xffBF79C2),
      ],
      shadowColor: const Color(0xff7F5ED9),
      shadowMaxOpacity: 0.05,
    ),
    startAngle: 180,
    angleRange: 360,
    size: 220,
    animDurationMultiplier: 0.3,
  );

  double _currentValue = 360;

  @override
  void didUpdateWidget(covariant BlockCountdown oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.blocksLeft == oldWidget.blocksLeft) return;

    if (_currentValue == 360) {
      _currentValue = 0;
      Future.delayed(const Duration(seconds: 1)).then((_) {
        if (mounted) {
          setState(() => _currentValue = 360);
        }
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return SleekCircularSlider(
      min: 0,
      max: 360,
      initialValue: _currentValue,
      appearance: _counterAppearance,
      innerWidget: (value) {
        return Transform.rotate(
          angle: degreesToRadians(value),
          child: Align(
            alignment: Alignment.center,
            child: Container(
              width: value / 4,
              height: value / 4,
              decoration: BoxDecoration(
                border: Border.all(color: Colors.white),
                borderRadius: BorderRadius.circular(4),
              ),
              alignment: Alignment.center,
              child: FittedBox(
                child: Text(
                  '${widget.blocksLeft}\nleft',
                  textAlign: TextAlign.center,
                  style: GoogleFonts.righteous(
                    color: Colors.white,
                    fontSize: 18,
                  ),
                ),
              ),
            ),
          ),
        );
      },
    );
  }
}
