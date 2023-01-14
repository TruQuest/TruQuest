import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:flutter_quill/flutter_quill.dart';

import '../../subject/models/im/subject_type_im.dart';

class DocumentContext {
  SubjectTypeIm? subjectType;
  String? subjectId;
  String? nameOrTitle;
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
    nameOrTitle = context.nameOrTitle;
    imageExt = context.imageExt;
    imageBytes = context.imageBytes;
    croppedImageBytes = context.croppedImageBytes;
    tags.addAll(context.tags);
    evidence.addAll(context.evidence);
  }
}
