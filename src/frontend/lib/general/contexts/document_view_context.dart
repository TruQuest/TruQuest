import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:flutter_quill/flutter_quill.dart';

import '../../settlement/models/rvm/settlement_proposal_vm.dart';
import '../../subject/models/rvm/subject_vm.dart';
import '../../widget_extensions.dart';
import '../../thing/models/rvm/thing_vm.dart';
import '../models/rvm/evidence_vm.dart';

class DocumentViewContext implements IDisposable {
  late final QuillController quillController;

  final SubjectVm? subject;
  final ThingVm? thing;
  final SettlementProposalVm? proposal;
  final List<EvidenceVm>? evidence;
  final String? signature;

  DocumentViewContext({
    required String nameOrTitle,
    required String details,
    this.subject,
    this.thing,
    this.proposal,
    this.evidence,
    this.signature,
  }) {
    quillController = QuillController(
      document: Document.fromJson(jsonDecode(details)),
      selection: const TextSelection.collapsed(offset: 0),
    );
  }

  @override
  void dispose() {
    // quillController.dispose();
  }
}
