import 'dart:js_util';
import 'dart:typed_data';

import 'package:flutter/material.dart';
import 'package:crop_your_image/crop_your_image.dart';
import 'package:tuple/tuple.dart';
import 'package:image/image.dart';

import '../../js.dart';

class ImageSelectionDialog extends StatefulWidget {
  const ImageSelectionDialog({super.key});

  @override
  State<ImageSelectionDialog> createState() => _ImageSelectionDialogState();
}

class _ImageSelectionDialogState extends State<ImageSelectionDialog> {
  final _textController = TextEditingController();
  final _cropController = CropController();

  Future<Tuple2<Uint8List, String>>? _future;
  String? _imageExt;
  Uint8List? _imageBytes;

  @override
  void dispose() {
    _textController.dispose();
    super.dispose();
  }

  Future<Tuple2<Uint8List, String>> _getImage(String url) async {
    var result = await promiseToFuture<ImageResult>(
      fetchAndResizeImage(url),
    );
    print(result.mimeType);
    var imageBytes = result.buffer.asUint8List();
    var mimeType = result.mimeType;

    String? ext;
    if (mimeType == 'image/png') {
      ext = 'png';
    } else if (mimeType == 'image/jpeg') {
      ext = 'jpg';
    } else {
      if (JpegDecoder().isValidFile(imageBytes)) {
        ext = 'jpg';
      } else if (PngDecoder().isValidFile(imageBytes)) {
        ext = 'png';
      }
    }

    print(ext);

    return Tuple2(imageBytes, ext!);
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text('Add image'),
      content: SizedBox(
        width: 960,
        height: 600,
        child: Column(
          children: [
            TextField(
              controller: _textController,
              decoration: InputDecoration(
                hintText: 'Paste url here',
                suffix: IconButton(
                  icon: Icon(Icons.file_download_rounded),
                  onPressed: () {
                    if (_textController.text.isNotEmpty) {
                      setState(() {
                        // @@TODO: Investigate what happens with discarded futures.
                        _future = _getImage(_textController.text);
                      });
                    }
                  },
                ),
              ),
            ),
            if (_future != null)
              Expanded(
                child: FutureBuilder(
                  future: _future,
                  builder: (context, snapshot) {
                    if (snapshot.data == null) {
                      return Center(child: CircularProgressIndicator());
                    }

                    _imageBytes = snapshot.data!.item1;
                    _imageExt = snapshot.data!.item2;
                    return Crop(
                      controller: _cropController,
                      image: _imageBytes!,
                      onCropped: (croppedImageData) {
                        Navigator.of(context).pop(
                          Tuple3(_imageExt!, _imageBytes!, croppedImageData),
                        );
                      },
                      withCircleUi: true,
                    );
                  },
                ),
              ),
          ],
        ),
      ),
      actions: [
        TextButton(
          child: Text('Ok'),
          onPressed: () {
            if (_imageBytes != null) {
              _cropController.crop();
            }
          },
        ),
      ],
    );
  }
}
