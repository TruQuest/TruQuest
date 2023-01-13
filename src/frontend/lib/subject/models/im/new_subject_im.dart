import 'dart:typed_data';

import 'package:dio/dio.dart';

import '../../../general/models/im/tag_im.dart';
import 'subject_type_im.dart';

class NewSubjectIm {
  final SubjectTypeIm type;
  final String name;
  final String details;
  final String imageExt;
  final Uint8List imageBytes;
  final String croppedImageExt;
  final Uint8List croppedImageBytes;
  final List<TagIm> tags;

  NewSubjectIm({
    required this.type,
    required this.name,
    required this.details,
    required this.imageExt,
    required this.imageBytes,
    required this.croppedImageExt,
    required this.croppedImageBytes,
    required this.tags,
  });

  FormData toFormData() {
    // @@??: Is order of sections guaranteed?
    return FormData.fromMap(
      {
        'type': type.index,
        'name': name,
        'details': details,
        'image': MultipartFile.fromBytes(
          imageBytes,
          filename: 'image.$imageExt',
        ),
        'croppedImage': MultipartFile.fromBytes(
          croppedImageBytes,
          filename: 'cropped_image.$croppedImageExt',
        ),
        'tags': tags.map((t) => t.id).join('|'),
      },
    );
  }
}
