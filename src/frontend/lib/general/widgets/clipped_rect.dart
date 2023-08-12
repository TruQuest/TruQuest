import 'package:flutter/material.dart';

class ClippedRect extends StatelessWidget {
  final double height;
  final Color color;
  final bool fromNarrowToWide;

  const ClippedRect({
    super.key,
    required this.height,
    required this.color,
    required this.fromNarrowToWide,
  });

  @override
  Widget build(BuildContext context) {
    return ClipPath(
      clipper: fromNarrowToWide ? ContainerClipperFromNarrowToWide() : ContainerClipper(),
      child: Container(
        color: color,
        width: 120,
        height: height,
      ),
    );
  }
}

class ContainerClipper extends CustomClipper<Path> {
  @override
  Path getClip(Size size) {
    var path = Path();
    path.lineTo(0, size.height);
    path.lineTo(size.width - (size.width * 0.4), size.height);
    path.lineTo(size.width, 0);
    path.close();

    return path;
  }

  @override
  bool shouldReclip(covariant CustomClipper<Path> oldClipper) => false;
}

class ContainerClipperFromNarrowToWide extends CustomClipper<Path> {
  @override
  Path getClip(Size size) {
    var path = Path();
    path.lineTo(0, size.height);
    path.lineTo(size.width, size.height);
    path.lineTo(size.width - (size.width * 0.55), 0);
    path.close();

    return path;
  }

  @override
  bool shouldReclip(covariant CustomClipper<Path> oldClipper) => false;
}
