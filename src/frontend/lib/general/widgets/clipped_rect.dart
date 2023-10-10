import 'package:flutter/material.dart';

class ClippedRect extends StatelessWidget {
  final double width;
  final double height;
  final Color color;
  final bool fromNarrowToWide;
  final double narrowSideFraction;
  final BorderRadiusGeometry? borderRadius;
  final Widget? child;

  const ClippedRect({
    super.key,
    this.width = 120,
    required this.height,
    required this.color,
    required this.fromNarrowToWide,
    required this.narrowSideFraction,
    this.borderRadius,
    this.child,
  });

  @override
  Widget build(BuildContext context) {
    return ClipPath(
      clipper: fromNarrowToWide
          ? ContainerClipperFromNarrowToWide(narrowSideFraction)
          : ContainerClipper(narrowSideFraction),
      child: Container(
        width: width,
        height: height,
        decoration: BoxDecoration(
          color: color,
          borderRadius: borderRadius,
        ),
        alignment: child != null ? Alignment(-0.53, 0) : null,
        child: child,
      ),
    );
  }
}

class ContainerClipper extends CustomClipper<Path> {
  final double narrowSideFraction;

  ContainerClipper(this.narrowSideFraction);

  @override
  Path getClip(Size size) {
    var path = Path();
    path.lineTo(0, size.height);
    path.lineTo(size.width - (size.width * narrowSideFraction), size.height);
    path.lineTo(size.width, 0);
    path.close();

    return path;
  }

  @override
  bool shouldReclip(covariant CustomClipper<Path> oldClipper) => false;
}

class ContainerClipperFromNarrowToWide extends CustomClipper<Path> {
  final double narrowSideFraction;

  ContainerClipperFromNarrowToWide(this.narrowSideFraction);

  @override
  Path getClip(Size size) {
    var path = Path();
    path.lineTo(0, size.height);
    path.lineTo(size.width, size.height);
    path.lineTo(size.width - (size.width * narrowSideFraction), 0);
    path.close();

    return path;
  }

  @override
  bool shouldReclip(covariant CustomClipper<Path> oldClipper) => false;
}
