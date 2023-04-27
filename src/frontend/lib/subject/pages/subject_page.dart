import 'package:flutter/material.dart';

import '../../general/contexts/document_context.dart';
import '../../general/widgets/evidence_block.dart';
import '../../general/widgets/prepare_draft_button.dart';
import '../../general/widgets/document_composer.dart';
import '../../general/widgets/image_block_with_crop.dart';
import '../../general/widgets/tags_block.dart';
import '../bloc/subject_bloc.dart';
import '../../widget_extensions.dart';

class SubjectPage extends StatelessWidgetX {
  late final _subjectBloc = use<SubjectBloc>();

  final String subjectId;

  SubjectPage({super.key, required this.subjectId});

  @override
  Widget buildX(BuildContext context) {
    return Scaffold(
      body: Center(child: Text('Subject')),
      floatingActionButton: FloatingActionButton(
        child: Icon(Icons.add),
        onPressed: () {
          var documentContext = DocumentContext();
          documentContext.subjectId = subjectId;

          showDialog(
            context: context,
            barrierDismissible: false,
            builder: (_) => ScopeX(
              useInstances: [documentContext],
              child: DocumentComposer(
                title: 'New thing',
                nameFieldLabel: 'Title',
                submitButton: PrepareDraftButton(),
                sideBlocks: [
                  ImageBlockWithCrop(cropCircle: false),
                  TagsBlock(),
                  EvidenceBlock(),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}
