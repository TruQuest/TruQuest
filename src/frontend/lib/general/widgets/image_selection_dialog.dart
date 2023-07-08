import 'dart:js_util';

import 'package:flutter/material.dart';
import 'package:crop_your_image/crop_your_image.dart';
import 'package:flutter/services.dart';
// ignore: depend_on_referenced_packages
import 'package:image/image.dart' hide Color;

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

  Future<(Uint8List, String)>? _getImageFuture;
  String? _imageExt;
  Uint8List? _imageBytes;

  @override
  void dispose() {
    _textController.dispose();
    _focusNode.dispose();
    super.dispose();
  }

  Future<(Uint8List, String)> _getImage(String url) async {
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

    return (imageBytes, ext);
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Add image'),
      content: SizedBox(
        width: 720,
        height: 450,
        child: Column(
          children: [
            TextField(
              controller: _textController,
              decoration: InputDecoration(
                hintText: 'Paste url here',
                suffix: IconButton(
                  icon: const Icon(Icons.file_download_rounded),
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
                      return const Center(child: CircularProgressIndicator());
                    }

                    _imageBytes = snapshot.data!.$1;
                    _imageExt = snapshot.data!.$2;
                    return Crop(
                      controller: _cropController,
                      image: _imageBytes!,
                      onCropped: (croppedImageData) {
                        Navigator.of(context).pop(
                          (_imageExt!, _imageBytes!, croppedImageData),
                        );
                      },
                      withCircleUi: widget.cropCircle,
                      aspectRatio: widget.cropCircle ? 1 : 16 / 9,
                    );
                  },
                ),
              ),
            if (_getImageFuture == null)
              Expanded(
                child: Container(
                  margin: const EdgeInsets.only(top: 16),
                  width: double.infinity,
                  height: double.infinity,
                  decoration: BoxDecoration(
                    border: Border.all(
                      color: Colors.grey[600]!,
                      width: 6,
                    ),
                  ),
                  alignment: Alignment.center,
                  child: Icon(
                    Icons.image,
                    size: 60,
                    color: Colors.grey[400],
                  ),
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
            style: TextButton.styleFrom(
              backgroundColor: const Color(0xFF242423),
              foregroundColor: Colors.white,
            ),
            child: const Text('Ok'),
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
