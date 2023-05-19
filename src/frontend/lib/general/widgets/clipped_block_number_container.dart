import 'package:flutter/material.dart';

class ClippedBlockNumberContainer extends StatelessWidget {
  final Color color;
  final double height;
  final Widget child;

  const ClippedBlockNumberContainer({
    super.key,
    required this.color,
    required this.height,
    required this.child,
  });

  @override
  Widget build(BuildContext context) {
    return ClipPath(
      clipper: ContainerClipper(),
      child: Container(
        width: 140,
        height: height,
        decoration: BoxDecoration(
          color: color,
          borderRadius: BorderRadius.only(
            topLeft: Radius.circular(12),
            bottomLeft: Radius.circular(12),
          ),
        ),
        alignment: Alignment(-0.2, 0),
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
    path.lineTo(size.width, size.height * 0.5);
    path.lineTo(size.width - (size.width / 3), 0);
    path.close();

    return path;
  }

  @override
  bool shouldReclip(covariant CustomClipper<Path> oldClipper) => false;
}
