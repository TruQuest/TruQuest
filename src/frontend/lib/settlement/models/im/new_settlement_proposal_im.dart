import 'dart:typed_data';

import 'package:dio/dio.dart';

import 'verdict_im.dart';
import 'supporting_evidence_im.dart';

class NewSettlementProposalIm {
  final String thingId;
  final String title;
  final VerdictIm verdict;
  final String details;
  final String? imageExt;
  final Uint8List? imageBytes;
  final String? croppedImageExt;
  final Uint8List? croppedImageBytes;
  final List<SupportingEvidenceIm> evidence;

  NewSettlementProposalIm({
    required this.thingId,
    required this.title,
    required this.verdict,
    required this.details,
    required this.imageExt,
    required this.imageBytes,
    required this.croppedImageExt,
    required this.croppedImageBytes,
    required this.evidence,
  });

  FormData toFormData() {
    // @@??: Is order of sections guaranteed?
    var map = <String, dynamic>{
      'thingId': thingId,
      'title': title,
      'verdict': verdict.index,
      'details': details,
      'evidence': evidence.map((e) => e.url).join('|'),
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
