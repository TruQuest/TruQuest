import 'package:flutter/material.dart';

class ClippedAvatarContainer extends StatelessWidget {
  final Widget child;
  final Color color;
  final bool fromNarrowToWide;

  const ClippedAvatarContainer({
    super.key,
    required this.child,
    required this.color,
    required this.fromNarrowToWide,
  });

  @override
  Widget build(BuildContext context) {
    return ClipPath(
      clipper: fromNarrowToWide
          ? ContainerClipperFromNarrowToWide()
          : ContainerClipper(),
      child: Container(
        width: 240,
        height: 200,
        decoration: BoxDecoration(
          color: color,
          borderRadius: BorderRadius.only(
            topLeft: Radius.circular(12),
            bottomLeft: Radius.circular(12),
          ),
        ),
        alignment: Alignment(-0.5, -0.2),
        child: child,
      ),
    );
  }
}

class ContainerClipper extends CustomClipper<Path> {
  @override
  Path getClip(Size size) {
    var path = Path();
    path.lineTo(0, size.height);
    path.lineTo(size.width - (size.width / 3), size.height);
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
    path.lineTo(size.width - (size.width / 3), 0);
    path.close();

    return path;
  }

  @override
  bool shouldReclip(covariant CustomClipper<Path> oldClipper) => false;
}
