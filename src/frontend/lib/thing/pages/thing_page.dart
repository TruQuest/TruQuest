import 'package:flutter/material.dart';
import 'package:tabbed_view/tabbed_view.dart';

import '../../general/widgets/tags_view_block.dart';
import '../widgets/state_transition_block.dart';
import '../../general/contexts/document_view_context.dart';
import '../../general/widgets/document_view.dart';
import '../bloc/thing_actions.dart';
import '../../widget_extensions.dart';
import '../bloc/thing_bloc.dart';

class ThingPage extends StatefulWidget {
  final String thingId;

  const ThingPage({super.key, required this.thingId});

  @override
  State<ThingPage> createState() => _ThingPageState();
}

class _ThingPageState extends StateX<ThingPage> {
  late final _thingBloc = use<ThingBloc>();

  @override
  void initState() {
    super.initState();
    _thingBloc.dispatch(GetThing(thingId: widget.thingId));
  }

  @override
  Widget build(BuildContext context) {
    return StreamBuilder(
      stream: _thingBloc.thing$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return Center(child: CircularProgressIndicator());
        }

        var vm = snapshot.data!;
        var thing = vm.thing;

        var documentViewContext = DocumentViewContext(
          nameOrTitle: thing.title,
          details: thing.details,
          tags: thing.tags.map((t) => t.name).toList(),
          thing: thing,
          signature: vm.signature,
        );

        // @@!!: Not being disposed of!
        var controller = TabbedViewController(
          [
            TabData(
              text: 'Details',
              content: UseScope(
                useInstances: [documentViewContext],
                preserveOnRebuild: false,
                child: DocumentView(
                  sideBlocks: [
                    TagsViewBlock(),
                    StateTransitionBlock(),
                  ],
                ),
              ),
              closable: false,
            ),
            TabData(
              text: 'Verifiers',
              content: Center(
                child: Text('Verifiers'),
              ),
              closable: false,
            ),
          ],
        );

        return TabbedView(controller: controller);
      },
    );
  }
}