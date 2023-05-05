import 'package:flutter/material.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

enum AvatarSize {
  small,
  medium,
  big,
}

class AvatarWithReputationGauge extends StatelessWidget {
  final String subjectId;
  final String subjectAvatarIpfsCid;
  final AvatarSize size;
  final Color color;

  const AvatarWithReputationGauge({
    super.key,
    required this.subjectId,
    required this.subjectAvatarIpfsCid,
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
                    ? 165
                    : 145,
            animationEnabled: false,
            customColors: CustomSliderColors(
              trackColor: color,
              progressBarColors: [
                Color(0xff5465ff),
                Color(0xff788bff),
                Color(0xffb0c1ff),
              ],
            ),
          ),
          min: 0,
          max: 100,
          initialValue: 75,
          innerWidget: (value) {
            int reputation = value.floor();
            return Align(
              alignment: Alignment.bottomLeft,
              child: Transform.translate(
                offset: size == AvatarSize.big
                    ? Offset(40, 0)
                    : Offset(
                        reputation < 10 ? 40 : 20,
                        20,
                      ),
                child: Text(
                  reputation.toString(),
                  style: TextStyle(
                    fontSize: 34,
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
                  ? 130
                  : 110,
          height: size == AvatarSize.big
              ? 210
              : size == AvatarSize.medium
                  ? 130
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
                      ? 65
                      : 55,
            ),
            color: Color(0xff4361ee),
          ),
        ),
        Container(
          width: size == AvatarSize.big
              ? 200
              : size == AvatarSize.medium
                  ? 120
                  : 100,
          height: size == AvatarSize.big
              ? 200
              : size == AvatarSize.medium
                  ? 120
                  : 100,
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(
              size == AvatarSize.big
                  ? 100
                  : size == AvatarSize.medium
                      ? 60
                      : 50,
            ),
          ),
          clipBehavior: Clip.antiAlias,
          child: Image.network(
            'http://localhost:8080/ipfs/' + subjectAvatarIpfsCid,
            fit: BoxFit.cover,
          ),
        ),
      ],
    );
  }
}
