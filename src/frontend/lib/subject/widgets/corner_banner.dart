import 'package:flutter/material.dart';

class CornerBanner extends StatelessWidget {
  final double size;
  final double cornerRadius;
  final Color color;
  final IconData icon;

  const CornerBanner({
    super.key,
    required this.size,
    required this.cornerRadius,
    required this.color,
    required this.icon,
  });

  @override
  Widget build(BuildContext context) {
    return ClipPath(
      clipper: CornerBannerClipper(),
      child: Container(
        width: size,
        height: size,
        decoration: BoxDecoration(
          color: color,
          borderRadius: BorderRadius.only(
            topLeft: Radius.circular(cornerRadius),
          ),
        ),
        alignment: Alignment(-0.65, -0.5),
        child: Icon(icon, size: 22),
      ),
    );
  }
}

class CornerBannerClipper extends CustomClipper<Path> {
  @override
  Path getClip(Size size) {
    var path = Path();
    path.lineTo(0, size.height);
    path.lineTo(size.width, 0);
    path.close();

    return path;
  }

  @override
  bool shouldReclip(covariant CustomClipper<Path> oldClipper) => false;
}
