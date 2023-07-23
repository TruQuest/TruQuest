import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';

class ClippedImage extends StatelessWidget {
  final String imageIpfsCid;
  final double width;
  final double height;
  final bool fromNarrowToWide;

  const ClippedImage({
    super.key,
    required this.imageIpfsCid,
    required this.width,
    required this.height,
    this.fromNarrowToWide = false,
  });

  @override
  Widget build(BuildContext context) {
    return ClipPath(
      clipper:
          fromNarrowToWide ? ImageClipperFromNarrowToWide() : ImageClipper(),
      child: Container(
        width: width,
        height: height,
        decoration: const BoxDecoration(
          borderRadius: BorderRadius.only(
            topLeft: Radius.circular(12),
            bottomLeft: Radius.circular(12),
          ),
        ),
        clipBehavior: Clip.antiAlias,
        child: Image.network(
          '${dotenv.env['IPFS_GATEWAY_URL']}/$imageIpfsCid',
          fit: BoxFit.cover,
        ),
      ),
    );
  }
}

class ImageClipper extends CustomClipper<Path> {
  @override
  Path getClip(Size size) {
    var path = Path();
    path.lineTo(0, size.height);
    path.lineTo(size.width - (size.width / 4), size.height);
    path.lineTo(size.width, 0);
    path.close();

    return path;
  }

  @override
  bool shouldReclip(covariant CustomClipper<Path> oldClipper) => false;
}

class ImageClipperFromNarrowToWide extends CustomClipper<Path> {
  @override
  Path getClip(Size size) {
    var path = Path();
    path.lineTo(0, size.height);
    path.lineTo(size.width, size.height);
    path.lineTo(size.width - (size.width / 4), 0);
    path.close();

    return path;
  }

  @override
  bool shouldReclip(covariant CustomClipper<Path> oldClipper) => false;
}
