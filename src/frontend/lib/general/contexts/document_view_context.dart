import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:flutter_quill/flutter_quill.dart';

import '../../widget_extensions.dart';
import '../../thing/models/rvm/thing_vm.dart';

class DocumentViewContext implements IDisposable {
  late final QuillController quillController;

  final List<String> tags;
  final ThingVm? thing;
  final String? signature;

  DocumentViewContext({
    required String nameOrTitle,
    required String details,
    required this.tags,
    this.thing,
    this.signature,
  }) {
    quillController = QuillController(
      document: Document.fromJson(jsonDecode(details)),
      selection: TextSelection.collapsed(offset: 0),
    );
  }

  @override
  void dispose() {
    quillController.dispose();
  }
}
