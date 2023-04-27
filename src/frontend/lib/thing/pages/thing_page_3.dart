import 'package:flutter/material.dart';
import 'package:tab_container/tab_container.dart';

import '../models/rvm/get_thing_rvm.dart';
import '../models/rvm/thing_vm.dart';
import '../../settlement/widgets/settlement_proposals.dart';
import '../models/rvm/thing_state_vm.dart';
import '../widgets/lottery.dart';
import '../../general/widgets/evidence_view_block.dart';
import '../../general/widgets/tags_view_block.dart';
import '../widgets/poll.dart';
import '../widgets/state_transition_block.dart';
import '../../general/contexts/document_view_context.dart';
import '../../general/widgets/document_view.dart';
import '../bloc/thing_actions.dart';
import '../../widget_extensions.dart';
import '../bloc/thing_bloc.dart';

class ThingPage3 extends StatefulWidget {
  final String thingId;

  const ThingPage3({super.key, required this.thingId});

  @override
  State<ThingPage3> createState() => _ThingPage3State();
}

class _ThingPage3State extends StateX<ThingPage3> {
  late final _thingBloc = use<ThingBloc>();

  @override
  void initState() {
    super.initState();
    _thingBloc.dispatch(GetThing(
      thingId: widget.thingId,
      subscribe: true,
    ));
  }

  @override
  void dispose() {
    _thingBloc.dispatch(UnsubscribeFromThing(thingId: widget.thingId));
    super.dispose();
  }

  List<Widget> _buildTabs(ThingVm thing) {
    var state = thing.state;
    var items = [Icon(Icons.content_paste)];

    if (state.index >= ThingStateVm.fundedAndVerifierLotteryInitiated.index) {
      items.add(Icon(Icons.people));
      if (state.index >= ThingStateVm.verifiersSelectedAndPollInitiated.index) {
        items.add(Icon(Icons.poll_outlined));
        if (state.index >= ThingStateVm.awaitingSettlement.index) {
          items.add(Icon(Icons.handshake));
        }
      }
    }

    return items;
  }

  List<Widget> _buildContent(GetThingRvm vm) {
    var thing = vm.thing;
    var state = thing.state;

    var items = <Widget>[
      ScopeX(
        updatesShouldNotify: true,
        useInstances: [
          DocumentViewContext(
            nameOrTitle: thing.title,
            details: thing.details,
            tags: thing.tags.map((t) => t.name).toList(),
            thing: thing,
            signature: vm.signature,
          ),
        ],
        child: DocumentView(
          sideBlocks: [
            TagsViewBlock(),
            StateTransitionBlock(),
          ],
          bottomBlock: EvidenceViewBlock(),
        ),
      ),
    ];

    if (state.index >= ThingStateVm.fundedAndVerifierLotteryInitiated.index) {
      items.add(Lottery(thing: thing));
      if (state.index >= ThingStateVm.verifiersSelectedAndPollInitiated.index) {
        items.add(Poll(thing: thing));
        if (state.index >= ThingStateVm.awaitingSettlement.index) {
          items.add(SettlementProposals(thingId: thing.id));
        }
      }
    }

    return items;
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
        var tabs = _buildTabs(vm.thing);

        return TabContainer(
          controller: TabContainerController(length: tabs.length),
          tabEnd: 0.3,
          color: Colors.purple,
          isStringTabs: false,
          tabEdge: TabEdge.right,
          tabs: tabs,
          children: _buildContent(vm),
        );
      },
    );
  }
}
