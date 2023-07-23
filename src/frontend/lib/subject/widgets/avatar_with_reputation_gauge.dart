import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

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

  @override
  Widget build(BuildContext context) {
    return Stack(
      alignment: Alignment.center,
      children: [
        SleekCircularSlider(
          appearance: CircularSliderAppearance(
            angleRange: 300,
            size: size == AvatarSize.big
                ? 270
                : size == AvatarSize.medium
                    ? 230
                    : 145,
            animationEnabled: false,
            customColors: CustomSliderColors(
              dotColor: Colors.transparent,
              trackColor: color.withOpacity(0.7),
              progressBarColors: const [
                // Color(0xff5465ff),
                // Color(0xff788bff),
                // Color(0xffb0c1ff),
                // Color(0xFF3E54AC),
                // Color(0xFF655DBB),
                // Color(0xFFBFACE2),
                Color(0xFFECF2FF),
                Color(0xFFE15FED),
                Color(0xFF9254C8),
                Color(0xFF332FD0),
              ],
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
                            reputation < 10 ? 40 : 20,
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
              color: Colors.white,
            ),
            borderRadius: BorderRadius.circular(
              size == AvatarSize.big
                  ? 105
                  : size == AvatarSize.medium
                      ? 90
                      : 55,
            ),
            color: const Color(0xff4361ee),
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
