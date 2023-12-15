import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

import '../../settlement/models/vm/verdict_vm.dart';

enum AvatarSize {
  small,
  medium,
  big,
}

class AvatarWithReputationGauge extends StatelessWidget {
  final String subjectId;
  final String subjectAvatarIpfsCid;
  final double value;
  final AvatarSize size;
  final Color color;

  const AvatarWithReputationGauge({
    super.key,
    required this.subjectId,
    required this.subjectAvatarIpfsCid,
    required this.value,
    required this.size,
    required this.color,
  });

  Color _getColor() {
    Color colorA;
    Color colorB;
    double t;

    if (value <= -40) {
      colorA = VerdictVm.asGoodAsMaliciousIntent.getColor();
      colorB = VerdictVm.noEffortWhatsoever.getColor();
      double delta = -100 - -40;
      t = (-100 - value) / delta;
    } else if (value <= 0) {
      colorA = VerdictVm.noEffortWhatsoever.getColor();
      colorB = VerdictVm.motionNotAction.getColor();
      double delta = -40;
      t = (-40 - value) / delta;
    } else if (value <= 40) {
      colorA = VerdictVm.motionNotAction.getColor();
      colorB = VerdictVm.aintGoodEnough.getColor();
      double delta = -40;
      t = (-value) / delta;
    } else if (value <= 75) {
      colorA = VerdictVm.aintGoodEnough.getColor();
      colorB = VerdictVm.guessItCounts.getColor();
      double delta = 40 - 75;
      t = (40 - value) / delta;
    } else {
      colorA = VerdictVm.guessItCounts.getColor();
      colorB = VerdictVm.delivered.getColor();
      double delta = 75 - 100;
      t = (75 - value) / delta;
    }

    assert(t >= 0 && t <= 1);

    return Color.lerp(colorA, colorB, t)!;
  }

  @override
  Widget build(BuildContext context) {
    var dotValue = (value / 10.0).round() + 10; // [0; 20]
    String? before;
    String? after;
    if (dotValue > 0) before = '.' * dotValue;
    if (dotValue < 20) after = '.' * (20 - dotValue);

    var barColor = _getColor();

    return Stack(
      alignment: Alignment.center,
      children: [
        Tooltip(
          decoration: BoxDecoration(
            color: Colors.white.withOpacity(0.8),
            border: Border.all(color: Colors.black),
          ),
          richMessage: TextSpan(
            children: [
              TextSpan(
                text: '-100  ',
                style: GoogleFonts.righteous(
                  color: VerdictVm.asGoodAsMaliciousIntent.getColor(),
                  fontSize: 16,
                ),
              ),
              if (before != null)
                TextSpan(
                  text: before,
                  style: GoogleFonts.righteous(
                    color: Colors.black,
                    fontSize: 16,
                  ),
                ),
              TextSpan(
                text: 'v',
                style: GoogleFonts.righteous(
                  color: Colors.black,
                  fontSize: 16,
                ),
              ),
              if (after != null)
                TextSpan(
                  text: after,
                  style: GoogleFonts.righteous(
                    color: Colors.black,
                    fontSize: 16,
                  ),
                ),
              TextSpan(
                text: '  100',
                style: GoogleFonts.righteous(
                  color: VerdictVm.delivered.getColor(),
                  fontSize: 16,
                ),
              ),
            ],
          ),
          child: SleekCircularSlider(
            appearance: CircularSliderAppearance(
              angleRange: 300,
              size: size == AvatarSize.big
                  ? 270
                  : size == AvatarSize.medium
                      ? 230
                      : 145,
              animationEnabled: false,
              customColors: CustomSliderColors(
                dynamicGradient: true,
                dotColor: Colors.transparent,
                trackColor: color.withOpacity(0.7),
                progressBarColors: [
                  HSVColor.fromColor(barColor).withValue(0.8).toColor(),
                  HSVColor.fromColor(barColor).withValue(1.0).toColor(),
                ],
                shadowColor: barColor.withOpacity(0.5),
                hideShadow: false,
              ),
            ),
            min: -100,
            max: 100,
            initialValue: value,
            innerWidget: (value) {
              int reputation = value.floor();
              return Align(
                alignment: Alignment.bottomLeft,
                child: Transform.translate(
                  offset: size == AvatarSize.big
                      ? const Offset(40, 0)
                      : size == AvatarSize.medium
                          ? const Offset(40, 4)
                          : Offset(
                              reputation.toString().length < 3 ? 30 : 15,
                              20,
                            ),
                  child: Text(
                    reputation.toString(),
                    style: GoogleFonts.righteous(
                      fontSize: size == AvatarSize.big ? 34 : 28,
                      color: color,
                    ),
                  ),
                ),
              );
            },
          ),
        ),
        Container(
          width: size == AvatarSize.big
              ? 210
              : size == AvatarSize.medium
                  ? 180
                  : 110,
          height: size == AvatarSize.big
              ? 210
              : size == AvatarSize.medium
                  ? 180
                  : 110,
          decoration: BoxDecoration(
            border: Border.all(
              width: 3,
              color: color,
            ),
            borderRadius: BorderRadius.circular(
              size == AvatarSize.big
                  ? 105
                  : size == AvatarSize.medium
                      ? 90
                      : 55,
            ),
            color: barColor,
          ),
        ),
        Container(
          width: size == AvatarSize.big
              ? 200
              : size == AvatarSize.medium
                  ? 170
                  : 100,
          height: size == AvatarSize.big
              ? 200
              : size == AvatarSize.medium
                  ? 170
                  : 100,
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(
              size == AvatarSize.big
                  ? 100
                  : size == AvatarSize.medium
                      ? 85
                      : 50,
            ),
          ),
          clipBehavior: Clip.antiAlias,
          child: Image.network(
            '${dotenv.env['IPFS_GATEWAY_URL']}/$subjectAvatarIpfsCid',
            fit: BoxFit.cover,
          ),
        ),
      ],
    );
  }
}
