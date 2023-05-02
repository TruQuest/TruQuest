import 'package:flutter/material.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

class AvatarWithReputationGauge extends StatelessWidget {
  final String subjectId;
  final String subjectName;
  final String subjectAvatarIpfsCid;

  const AvatarWithReputationGauge({
    super.key,
    required this.subjectId,
    required this.subjectName,
    required this.subjectAvatarIpfsCid,
  });

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 16),
        child: Column(
          children: [
            Card(
              color: Colors.deepOrange[600],
              elevation: 5,
              child: Container(
                width: double.infinity,
                height: 30,
                alignment: Alignment.center,
                child: Text(
                  subjectName,
                  style: TextStyle(color: Colors.white),
                ),
              ),
            ),
            SizedBox(height: 12),
            Stack(
              alignment: Alignment.center,
              children: [
                SleekCircularSlider(
                  appearance: CircularSliderAppearance(
                    angleRange: 300,
                    size: 165,
                    animationEnabled: false,
                    customColors: CustomSliderColors(
                      trackColor: Colors.white70,
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
                        offset: Offset(
                          reputation < 10 ? 40 : 20,
                          20,
                        ),
                        child: Text(
                          reputation.toString(),
                          style: TextStyle(
                            fontSize: 34,
                            color: Colors.white,
                          ),
                        ),
                      ),
                    );
                  },
                ),
                Container(
                  width: 130,
                  height: 130,
                  decoration: BoxDecoration(
                    border: Border.all(
                      width: 3,
                      color: Colors.white,
                    ),
                    borderRadius: BorderRadius.circular(65),
                    color: Color(0xff4361ee),
                  ),
                ),
                Container(
                  width: 120,
                  height: 120,
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(60),
                  ),
                  clipBehavior: Clip.antiAlias,
                  child: Image.network(
                    'http://localhost:8080/ipfs/' + subjectAvatarIpfsCid,
                    fit: BoxFit.cover,
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
