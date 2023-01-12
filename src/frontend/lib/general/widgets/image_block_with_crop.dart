import 'dart:async';
import 'dart:typed_data';

import 'package:flutter/material.dart';
import 'package:crop_your_image/crop_your_image.dart';
import 'package:universal_html/js_util.dart';

import '../../js.dart';

class ImageBlockWithCrop extends StatefulWidget {
  const ImageBlockWithCrop({super.key});

  @override
  State<ImageBlockWithCrop> createState() => _ImageBlockWithCropState();
}

class _ImageBlockWithCropState extends State<ImageBlockWithCrop> {
  Future<Uint8List> _get() async {
    var result = await promiseToFuture<ByteBuffer>(
      fetchAndResizeImage(
        'http://localhost:8080/ipfs/QmVckD4usX1Kr5GhakWMXXeEuPn673yy8CrXLUDLetUjW2?filename=1c901c20-eb8b-4d1e-8890-83bac6be8c6b.png',
      ),
    );

    return result.asUint8List();
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        OutlinedButton(
          style: OutlinedButton.styleFrom(
            backgroundColor: Colors.yellow[700],
            foregroundColor: Colors.white,
            elevation: 5,
          ),
          child: Row(
            children: [
              Spacer(),
              Text('Image'),
              Expanded(
                child: Align(
                  alignment: Alignment.centerRight,
                  child: Icon(Icons.add),
                ),
              ),
            ],
          ),
          onPressed: () {
            showDialog(
              context: context,
              barrierDismissible: true,
              builder: (_) => AlertDialog(
                title: Text('Add image'),
                content: FutureBuilder(
                  future: _get(),
                  builder: (context, snapshot) {
                    if (snapshot.data == null) {
                      return CircularProgressIndicator();
                    }

                    return SizedBox(
                      width: 800,
                      height: 600,
                      child: Crop(
                        image: snapshot.data!,
                        onCropped: (croppedImageData) {},
                        withCircleUi: true,
                      ),
                    );
                  },
                ),
              ),
            );
          },
        ),
        SizedBox(height: 6),
        AspectRatio(
          aspectRatio: 16.0 / 9.0,
          child: Container(
            decoration: BoxDecoration(
              borderRadius: BorderRadius.all(Radius.circular(6)),
            ),
            clipBehavior: Clip.antiAlias,
            child: Image.network(
              'http://localhost:8080/ipfs/QmZM2spx66FYLxuitRmnhLU2HC436Cd9Z4AgoUGmPshoNq?filename=3cb8812e-177f-404d-8550-6bc47d3b144e.png',
              fit: BoxFit.cover,
            ),
          ),
        ),
      ],
    );
  }
}
