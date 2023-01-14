import 'package:flutter/material.dart';

import '../bloc/subject_result_vm.dart';
import '../bloc/subject_actions.dart';
import '../../widget_extensions.dart';
import '../../general/contexts/document_context.dart';
import '../bloc/subject_bloc.dart';

class SubmitButton extends StatefulWidget {
  const SubmitButton({super.key});

  @override
  State<SubmitButton> createState() => _SubmitButtonState();
}

class _SubmitButtonState extends StateX<SubmitButton> {
  late final _documentContext = useScoped<DocumentContext>();
  late final _subjectBloc = use<SubjectBloc>();

  bool _enabled = true;

  @override
  Widget build(BuildContext context) {
    return ElevatedButton(
      style: ElevatedButton.styleFrom(
        backgroundColor: Colors.amber[900],
        elevation: 8,
        padding: const EdgeInsets.symmetric(
          horizontal: 36,
          vertical: 16,
        ),
      ),
      child:
          _enabled ? Text('Submit') : CircularProgressIndicator(strokeWidth: 2),
      onPressed: _enabled
          ? () async {
              var action = AddNewSubject(
                documentContext: DocumentContext.fromEditable(_documentContext),
              );
              _subjectBloc.dispatch(action);

              setState(() {
                _enabled = false;
              });

              AddNewSubjectSuccessResult? vm = await action.result;
              if (vm != null) {
                Navigator.of(this.context).pop('/subjects/${vm.subjectId}');
              }
            }
          : null,
    );
  }
}
