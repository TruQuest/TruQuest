import 'dart:typed_data';

import 'package:flutter/material.dart';

import 'image_selection_dialog.dart';
import '../contexts/document_context.dart';
import '../../widget_extensions.dart';

class ImageBlockWithCrop extends StatefulWidget {
  final bool cropCircle;

  const ImageBlockWithCrop({super.key, required this.cropCircle});

  @override
  State<ImageBlockWithCrop> createState() => _ImageBlockWithCropState();
}

class _ImageBlockWithCropState extends StateX<ImageBlockWithCrop> {
  late final _documentContext = useScoped<DocumentContext>();

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
          onPressed: () async {
            var result = await showDialog<(String, Uint8List, Uint8List)>(
              context: context,
              builder: (_) => ImageSelectionDialog(
                cropCircle: widget.cropCircle,
              ),
            );
            if (result != null) {
              setState(() {
                _documentContext.imageExt = result.$1;
                _documentContext.imageBytes = result.$2;
                _documentContext.croppedImageBytes = result.$3;
              });
            }
          },
        ),
        if (_documentContext.croppedImageBytes != null)
          Padding(
            padding: const EdgeInsets.only(top: 6),
            child: widget.cropCircle
                ? CircleAvatar(
                    radius: 30,
                    foregroundImage: MemoryImage(
                      _documentContext.croppedImageBytes!,
                    ),
                  )
                : AspectRatio(
                    aspectRatio: 16 / 9,
                    child: Container(
                      decoration: BoxDecoration(
                        borderRadius: BorderRadius.circular(6),
                      ),
                      clipBehavior: Clip.antiAlias,
                      child: Image.memory(
                        _documentContext.croppedImageBytes!,
                        fit: BoxFit.cover,
                      ),
                    ),
                  ),
          ),
      ],
    );
  }
}
