import 'package:flutter/material.dart';

class ClippedBlockNumberContainer extends StatelessWidget {
  final Widget child;

  const ClippedBlockNumberContainer({
    super.key,
    required this.child,
  });

  @override
  Widget build(BuildContext context) {
    return ClipPath(
      clipper: ContainerClipper(),
      child: Container(
        width: 140,
        height: 120,
        decoration: BoxDecoration(
          color: Colors.blueAccent,
          borderRadius: BorderRadius.only(
            topLeft: Radius.circular(12),
            bottomLeft: Radius.circular(12),
          ),
        ),
        alignment: Alignment(-0.25, 0),
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
