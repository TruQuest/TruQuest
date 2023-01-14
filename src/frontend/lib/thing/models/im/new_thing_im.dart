import 'dart:typed_data';

import 'package:dio/dio.dart';

import '../../../general/models/im/tag_im.dart';
import 'evidence_im.dart';

class NewThingIm {
  final String subjectId;
  final String title;
  final String details;
  final String? imageExt;
  final Uint8List? imageBytes;
  final String? croppedImageExt;
  final Uint8List? croppedImageBytes;
  final List<EvidenceIm> evidence;
  final List<TagIm> tags;

  NewThingIm({
    required this.subjectId,
    required this.title,
    required this.details,
    required this.imageExt,
    required this.imageBytes,
    required this.croppedImageExt,
    required this.croppedImageBytes,
    required this.evidence,
    required this.tags,
  });

  FormData toFormData() {
    // @@??: Is order of sections guaranteed?
    var map = <String, dynamic>{
      'subjectId': subjectId,
      'title': title,
      'details': details,
      'evidence': evidence.map((e) => e.url).join('|'),
      'tags': tags.map((t) => t.id).join('|'),
    };

    if (imageBytes != null && croppedImageBytes != null) {
      map['image'] = MultipartFile.fromBytes(
        imageBytes!,
        filename: 'image.$imageExt',
      );
      map['croppedImage'] = MultipartFile.fromBytes(
        croppedImageBytes!,
        filename: 'cropped_image.$croppedImageExt',
      );
    }

    return FormData.fromMap(map);
  }
}
