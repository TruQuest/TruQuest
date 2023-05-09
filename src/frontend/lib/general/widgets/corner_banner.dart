import 'package:flutter/material.dart';

class CornerBanner extends StatelessWidget {
  final Alignment position;
  final double size;
  final double cornerRadius;
  final Color color;
  final Widget child;

  const CornerBanner({
    super.key,
    required this.position,
    required this.size,
    required this.cornerRadius,
    required this.color,
    required this.child,
  });

  @override
  Widget build(BuildContext context) {
    return ClipPath(
      clipper: position == Alignment.topLeft
          ? TopLeftCornerBannerClipper()
          : BottomRightCornerBannerClipper(),
      child: Container(
        width: size,
        height: size,
        decoration: BoxDecoration(
          color: color,
          borderRadius: BorderRadius.only(
            topLeft: position == Alignment.topLeft
                ? Radius.circular(cornerRadius)
                : Radius.zero,
            bottomRight: position == Alignment.bottomRight
                ? Radius.circular(cornerRadius)
                : Radius.zero,
          ),
        ),
        alignment: position == Alignment.topLeft
            ? Alignment(-0.65, -0.5)
            : Alignment(0.65, 0.5),
        child: child,
      ),
    );
  }
}

class TopLeftCornerBannerClipper extends CustomClipper<Path> {
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

class BottomRightCornerBannerClipper extends CustomClipper<Path> {
  @override
  Path getClip(Size size) {
    var path = Path();
    path.moveTo(0, size.height);
    path.lineTo(size.width, size.height);
    path.lineTo(size.width, 0);
    path.close();

    return path;
  }

  @override
  bool shouldReclip(covariant CustomClipper<Path> oldClipper) => false;
}
