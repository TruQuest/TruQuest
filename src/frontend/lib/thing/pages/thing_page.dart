import 'package:flutter/material.dart';

import '../bloc/thing_actions.dart';
import '../bloc/thing_result_vm.dart';
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
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: Text('Thing ${widget.thingId}'),
      ),
      floatingActionButton: FloatingActionButton(
        child: Text('Submit'),
        onPressed: () async {
          var action = SubmitNewThing(thingId: widget.thingId);
          _thingBloc.dispatch(action);

          SubmitNewThingSuccessVm? vm = await action.result;
          if (vm != null) {}
        },
      ),
    );
  }
}
