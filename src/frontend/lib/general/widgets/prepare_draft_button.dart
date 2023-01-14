import 'package:flutter/material.dart';

import '../../thing/bloc/thing_result_vm.dart';
import '../../thing/bloc/thing_actions.dart';
import '../contexts/document_context.dart';
import '../../thing/bloc/thing_bloc.dart';
import '../../widget_extensions.dart';

class PrepareDraftButton extends StatefulWidget {
  const PrepareDraftButton({super.key});

  @override
  State<PrepareDraftButton> createState() => _PrepareDraftButtonState();
}

class _PrepareDraftButtonState extends StateX<PrepareDraftButton> {
  late final _documentContext = useScoped<DocumentContext>();
  late final _thingBloc = use<ThingBloc>();

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
      child: _enabled
          ? Text('Prepare draft')
          : CircularProgressIndicator(strokeWidth: 2),
      onPressed: _enabled
          ? () async {
              var action = CreateNewThingDraft(
                documentContext: DocumentContext.fromEditable(_documentContext),
              );
              _thingBloc.dispatch(action);

              setState(() {
                _enabled = false;
              });

              var vm = await action.result;
              if (vm is CreateNewThingDraftFailureVm) {
                // ...
                return;
              }

              vm as CreateNewThingDraftSuccessVm;
              Navigator.of(this.context).pop();
            }
          : null,
    );
  }
}
