import 'package:flutter/material.dart';
import 'package:tabbed_view/tabbed_view.dart';

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

class ThingPage1 extends StatefulWidget {
  final String thingId;

  const ThingPage1({super.key, required this.thingId});

  @override
  State<ThingPage1> createState() => _ThingPage1State();
}

class _ThingPage1State extends StateX<ThingPage1> {
  late final _thingBloc = use<ThingBloc>();

  final _tabController = TabbedViewController([]);

  @override
  void initState() {
    super.initState();

    _thingBloc.dispatch(GetThing(
      thingId: widget.thingId,
      subscribe: true,
    ));

    _thingBloc.thing$.listen((result) {
      var thing = result.thing;

      var documentViewContext = DocumentViewContext(
        nameOrTitle: thing.title,
        details: thing.details,
        tags: thing.tags.map((t) => t.name).toList(),
        thing: thing,
        signature: result.signature,
      );

      Widget content = ScopeX(
        useInstances: [documentViewContext],
        updatesShouldNotify: true,
        child: DocumentView(
          sideBlocks: [
            TagsViewBlock(),
            StateTransitionBlock(),
          ],
          bottomBlock: EvidenceViewBlock(),
        ),
      );

      if (_tabController.tabs.isNotEmpty) {
        _tabController.tabs[0].content = null;
        _tabController.tabs[0].content = content;
      } else {
        _tabController.addTab(
          TabData(
            text: 'Details',
            content: content,
            closable: false,
            buttons: [
              TabButton(
                icon: IconProvider.data(Icons.refresh),
                onPressed: () {
                  _thingBloc.dispatch(GetThing(thingId: widget.thingId));
                },
              ),
            ],
          ),
        );
      }

      if (thing.state.index >=
          ThingStateVm.fundedAndVerifierLotteryInitiated.index) {
        content = Lottery(thing: thing);
        if (_tabController.tabs.length > 1) {
          _tabController.tabs[1].content = content;
        } else {
          _tabController.addTab(
            TabData(
              text: 'Verifier Lottery',
              content: content,
              closable: false,
            ),
          );
        }

        if (thing.state.index >=
            ThingStateVm.verifiersSelectedAndPollInitiated.index) {
          content = Poll(thing: thing);
          if (_tabController.tabs.length > 2) {
            _tabController.tabs[2].content = content;
          } else {
            _tabController.addTab(
              TabData(
                text: 'Acceptance Poll',
                content: content,
                closable: false,
              ),
            );
          }

          if (thing.state.index >= ThingStateVm.awaitingSettlement.index) {
            content = SettlementProposals(thingId: thing.id);
            if (_tabController.tabs.length > 3) {
              _tabController.tabs[3].content = content;
            } else {
              _tabController.addTab(
                TabData(
                  text: 'Settlement proposals',
                  content: content,
                  closable: false,
                ),
              );
            }
          }
        }
      }
    });
  }

  @override
  void dispose() {
    _thingBloc.dispatch(UnsubscribeFromThing(thingId: widget.thingId));
    _tabController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return TabbedView(controller: _tabController);
  }
}
