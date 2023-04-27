import 'package:flutter/material.dart';
import 'package:backdrop/backdrop.dart';

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

class ThingPage2 extends StatefulWidget {
  final String thingId;

  const ThingPage2({super.key, required this.thingId});

  @override
  State<ThingPage2> createState() => _ThingPage2State();
}

class _ThingPage2State extends StateX<ThingPage2> {
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

  @override
  Widget build(BuildContext context) {
    return StreamBuilder(
      stream: _thingBloc.thing$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return Center(child: CircularProgressIndicator());
        }

        var vm = snapshot.data!;
        return Backdrop(
          thing: vm.thing,
          signature: vm.signature,
        );
      },
    );
  }
}

class Backdrop extends StatefulWidget {
  final ThingVm thing;
  final String? signature;

  const Backdrop({
    super.key,
    required this.thing,
    required this.signature,
  });

  @override
  State<Backdrop> createState() => _BackdropState();
}

class _BackdropState extends State<Backdrop> {
  late DocumentViewContext _documentViewContext;

  int _selectedPage = 0;

  void _setDocumentViewContext() {
    _documentViewContext = DocumentViewContext(
      nameOrTitle: widget.thing.title,
      details: widget.thing.details,
      tags: widget.thing.tags.map((t) => t.name).toList(),
      thing: widget.thing,
      signature: widget.signature,
    );
  }

  @override
  void initState() {
    super.initState();
    _setDocumentViewContext();
  }

  @override
  void didUpdateWidget(covariant Backdrop oldWidget) {
    super.didUpdateWidget(oldWidget);
    _setDocumentViewContext();
  }

  Widget _buildTitle() {
    switch (_selectedPage) {
      case 0:
        return Text('Details');
      case 1:
        return Text('Verifier Lottery');
      case 2:
        return Text('Acceptance Poll');
      case 3:
        return Text('Settlement Proposals');
    }

    throw UnimplementedError();
  }

  Widget _buildBackLayer() {
    var state = widget.thing.state;
    var items = [
      ListTile(title: Text('Details')),
    ];

    if (state.index >= ThingStateVm.fundedAndVerifierLotteryInitiated.index) {
      items.add(ListTile(title: Text('Verifier Lottery')));
      if (state.index >= ThingStateVm.verifiersSelectedAndPollInitiated.index) {
        items.add(ListTile(title: Text('Settlement Proposals')));
        if (state.index >= ThingStateVm.awaitingSettlement.index) {
          items.add(ListTile(title: Text('Acceptance Poll')));
        }
      }
    }

    return BackdropNavigationBackLayer(
      items: items,
      onTap: (value) => setState(() {
        _selectedPage = value;
      }),
      separatorBuilder: (_, __) => Divider(),
    );
  }

  Widget _buildFrontLayer() {
    switch (_selectedPage) {
      case 0:
        return ScopeX(
          useInstances: [_documentViewContext],
          updatesShouldNotify: true,
          child: DocumentView(
            sideBlocks: [
              TagsViewBlock(),
              StateTransitionBlock(),
            ],
            bottomBlock: EvidenceViewBlock(),
          ),
        );
      case 1:
        return Lottery(thing: widget.thing);
      case 2:
        return Poll(thing: widget.thing);
      case 3:
        return SettlementProposals(thingId: widget.thing.id);
    }

    throw UnimplementedError();
  }

  @override
  Widget build(BuildContext context) {
    return BackdropScaffold(
      appBar: BackdropAppBar(
        title: _buildTitle(),
      ),
      stickyFrontLayer: true,
      backLayer: _buildBackLayer(),
      frontLayer: _buildFrontLayer(),
    );
  }
}
