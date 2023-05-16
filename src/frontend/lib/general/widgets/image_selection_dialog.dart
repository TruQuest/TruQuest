import 'dart:js_util';

import 'package:flutter/material.dart';
import 'package:crop_your_image/crop_your_image.dart';
import 'package:flutter/services.dart';
import 'package:tuple/tuple.dart';
import 'package:image/image.dart';

import '../../js.dart';

class ImageSelectionDialog extends StatefulWidget {
  final bool cropCircle;

  const ImageSelectionDialog({super.key, required this.cropCircle});

  @override
  State<ImageSelectionDialog> createState() => _ImageSelectionDialogState();
}

class _ImageSelectionDialogState extends State<ImageSelectionDialog> {
  final _textController = TextEditingController();
  final _cropController = CropController();
  final _focusNode = FocusNode();

  Future<Tuple2<Uint8List, String>>? _getImageFuture;
  String? _imageExt;
  Uint8List? _imageBytes;

  @override
  void dispose() {
    _textController.dispose();
    _focusNode.dispose();
    super.dispose();
  }

  Future<Tuple2<Uint8List, String>> _getImage(String url) async {
    var result = await promiseToFuture<ImageResult>(
      fetchAndResizeImage(url),
    );
    var imageBytes = result.buffer.asUint8List();
    var mimeType = result.mimeType;

    final String ext;
    if (mimeType == 'image/png') {
      ext = 'png';
    } else if (mimeType == 'image/jpeg') {
      ext = 'jpg';
    } else {
      if (JpegDecoder().isValidFile(imageBytes)) {
        ext = 'jpg';
      } else if (PngDecoder().isValidFile(imageBytes)) {
        ext = 'png';
      } else {
        throw UnimplementedError();
      }
    }

    return Tuple2(imageBytes, ext);
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
                        _getImageFuture = _getImage(_textController.text);
                        _focusNode.requestFocus();
                      });
                    }
                  },
                ),
              ),
            ),
            if (_getImageFuture != null)
              Expanded(
                child: FutureBuilder(
                  future: _getImageFuture,
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
                      withCircleUi: widget.cropCircle,
                      aspectRatio: widget.cropCircle ? 1 : 16 / 9,
                    );
                  },
                ),
              ),
          ],
        ),
      ),
      actions: [
        KeyboardListener(
          focusNode: _focusNode,
          autofocus: false,
          onKeyEvent: (event) {
            if (event.logicalKey == LogicalKeyboardKey.enter &&
                event is KeyUpEvent) {
              if (_imageBytes != null) {
                _cropController.crop();
              }
            }
          },
          child: TextButton(
            child: Text('Ok'),
            onPressed: () {
              if (_imageBytes != null) {
                _cropController.crop();
              }
            },
          ),
        ),
      ],
    );
  }
}
