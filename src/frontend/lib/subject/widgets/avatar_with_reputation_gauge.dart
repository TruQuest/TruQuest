import 'package:flutter/material.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

class AvatarWithReputationGauge extends StatelessWidget {
  final String subjectId;
  final String subjectAvatarIpfsCid;
  final bool big;
  final Color color;

  const AvatarWithReputationGauge({
    super.key,
    required this.subjectId,
    required this.subjectAvatarIpfsCid,
    required this.big,
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
            size: big ? 270 : 165,
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
                offset: big
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
          width: big ? 210 : 130,
          height: big ? 210 : 130,
          decoration: BoxDecoration(
            border: Border.all(
              width: 3,
              color: Colors.white,
            ),
            borderRadius: BorderRadius.circular(big ? 105 : 65),
            color: Color(0xff4361ee),
          ),
        ),
        Container(
          width: big ? 200 : 120,
          height: big ? 200 : 120,
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(big ? 100 : 60),
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
