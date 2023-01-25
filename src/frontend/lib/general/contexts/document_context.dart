import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:flutter_quill/flutter_quill.dart';

import '../../settlement/models/im/verdict_im.dart';
import '../../subject/models/im/subject_type_im.dart';

class DocumentContext {
  SubjectTypeIm? subjectType;
  String? subjectId;
  String? thingId;
  String? nameOrTitle;
  VerdictIm? verdict;
  final QuillController? quillController;
  final String? details;
  String? imageExt;
  Uint8List? imageBytes;
  Uint8List? croppedImageBytes;
  final List<String> tags = [];
  final List<String> evidence = [];

  DocumentContext()
      : quillController = QuillController.basic(),
        details = null;

  DocumentContext.fromEditable(DocumentContext context)
      : quillController = null,
        details =
            jsonEncode(context.quillController!.document.toDelta().toJson()) {
    subjectType = context.subjectType;
    subjectId = context.subjectId;
    thingId = context.thingId;
    nameOrTitle = context.nameOrTitle;
    verdict = context.verdict;
    imageExt = context.imageExt;
    imageBytes = context.imageBytes;
    croppedImageBytes = context.croppedImageBytes;
    tags.addAll(context.tags);
    evidence.addAll(context.evidence);
  }
}
