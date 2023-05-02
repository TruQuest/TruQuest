import 'package:flutter/material.dart';

class Poster extends StatelessWidget {
  static const posterAspectRatio = 16 / 9;

  final String imageIpfsCid;
  final double height;

  Poster(
    this.imageIpfsCid, {
    super.key,
    this.height = 200,
  });

  @override
  Widget build(BuildContext context) {
    var width = posterAspectRatio * height;

    return Material(
      borderRadius: BorderRadius.circular(12),
      elevation: 10,
      clipBehavior: Clip.antiAlias,
      child: Image.network(
        'http://localhost:8080/ipfs/' + imageIpfsCid,
        width: width,
        height: height,
        fit: BoxFit.cover,
      ),
    );
  }
}
