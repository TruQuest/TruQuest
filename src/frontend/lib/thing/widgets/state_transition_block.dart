import 'package:flutter/material.dart';

import '../bloc/thing_actions.dart';
import '../bloc/thing_bloc.dart';
import '../models/rvm/thing_state_vm.dart';
import '../../general/contexts/document_view_context.dart';
import '../../widget_extensions.dart';

class StateTransitionBlock extends StatefulWidget {
  const StateTransitionBlock({super.key});

  @override
  State<StateTransitionBlock> createState() => _StateTransitionBlockState();
}

class _StateTransitionBlockState extends StateX<StateTransitionBlock> {
  late final _documentViewContext = useScoped<DocumentViewContext>();
  late final _thingBloc = use<ThingBloc>();

  Widget _buildLabelsAndActions() {
    var thing = _documentViewContext.thing!;
    if (thing.state == ThingStateVm.draft) {
      return Column(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          OutlinedButton(
            style: OutlinedButton.styleFrom(
              foregroundColor: Colors.purple[800],
              side: BorderSide(color: Colors.purple[800]!),
              elevation: 0,
            ),
            child: Text('Submit'),
            onPressed: () {
              _thingBloc.dispatch(SubmitNewThing(thing: thing));
            },
          ),
          Container(
            color: Colors.black,
            width: 2,
            height: 20,
          ),
          OutlinedButton(
            style: OutlinedButton.styleFrom(
              disabledForegroundColor: Colors.red[200],
              side: BorderSide(color: Colors.red[200]!),
              elevation: 0,
            ),
            child: Text('Fund'),
            onPressed: null,
          ),
        ],
      );
    } else if (thing.state == ThingStateVm.awaitingFunding) {
      return Column(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          OutlinedButton(
            style: OutlinedButton.styleFrom(
              disabledForegroundColor: Colors.purple[200],
              side: BorderSide(color: Colors.purple[200]!),
              elevation: 0,
            ),
            child: Text('Submitted'),
            onPressed: null,
          ),
          Container(
            color: Colors.black,
            width: 2,
            height: 20,
          ),
          OutlinedButton(
            style: OutlinedButton.styleFrom(
              foregroundColor: Colors.red[600],
              side: BorderSide(color: Colors.red[600]!),
              elevation: 0,
            ),
            child: Text('Fund'),
            onPressed: () {
              _thingBloc.dispatch(
                FundThing(
                  thing: thing,
                  signature: _documentViewContext.signature!,
                ),
              );
            },
          ),
        ],
      );
    } else if (thing.state ==
        ThingStateVm.fundedAndSubmissionVerifierLotteryInitiated) {
      return Column(
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          OutlinedButton(
            style: OutlinedButton.styleFrom(
              disabledForegroundColor: Colors.purple[200],
              side: BorderSide(color: Colors.purple[200]!),
              elevation: 0,
            ),
            child: Text('Submitted'),
            onPressed: null,
          ),
          Container(
            color: Colors.black,
            width: 2,
            height: 20,
          ),
          OutlinedButton(
            style: OutlinedButton.styleFrom(
              disabledForegroundColor: Colors.red[200],
              side: BorderSide(color: Colors.red[200]!),
              elevation: 0,
            ),
            child: Text('Funded'),
            onPressed: null,
          ),
        ],
      );
    }

    return Container();
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Card(
          color: Colors.indigo,
          elevation: 5,
          child: Container(
            width: double.infinity,
            height: 30,
            alignment: Alignment.center,
            child: Text(
              'Status',
              style: TextStyle(color: Colors.white),
            ),
          ),
        ),
        SizedBox(height: 6),
        Card(
          child: _buildLabelsAndActions(),
        ),
      ],
    );
  }
}
