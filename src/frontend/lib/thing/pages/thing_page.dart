import 'dart:async';

import 'package:flutter/material.dart';
import 'package:sliver_tools/sliver_tools.dart';
import 'package:tab_container/tab_container.dart';

import '../widgets/status_stepper_block.dart';
import '../bloc/thing_result_vm.dart';
import '../../user/bloc/user_result_vm.dart';
import '../../general/widgets/watch_button.dart';
import '../../subject/widgets/avatar_with_reputation_gauge.dart';
import '../../user/bloc/user_bloc.dart';
import '../widgets/settlement_proposals_list.dart';
import '../../general/widgets/arc_banner_image.dart';
import '../../general/widgets/poster.dart';
import '../models/rvm/get_thing_rvm.dart';
import '../models/rvm/thing_vm.dart';
import '../models/rvm/thing_state_vm.dart';
import '../widgets/lottery.dart';
import '../../general/widgets/evidence_view_block.dart';
import '../widgets/poll.dart';
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
  late final _userBloc = use<UserBloc>();
  late final _thingBloc = use<ThingBloc>();

  late final StreamSubscription<LoadCurrentUserSuccessVm> _currentUser$$;

  @override
  void initState() {
    super.initState();
    _currentUser$$ = _userBloc.currentUser$.listen(
      (_) => _thingBloc.dispatch(GetThing(thingId: widget.thingId)),
    );
  }

  @override
  void dispose() {
    super.dispose();
    _currentUser$$.cancel();
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

  List<Widget> _buildTabContents(GetThingRvm vm) {
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
          rightSideBlocks: [
            Center(
              child: Padding(
                padding: const EdgeInsets.symmetric(vertical: 16),
                child: Column(
                  children: [
                    Card(
                      color: Colors.deepOrange[600],
                      elevation: 5,
                      child: Container(
                        width: double.infinity,
                        height: 30,
                        alignment: Alignment.center,
                        child: Text(
                          thing.subjectName,
                          style: TextStyle(color: Colors.white),
                        ),
                      ),
                    ),
                    SizedBox(height: 12),
                    AvatarWithReputationGauge(
                      subjectId: thing.subjectId,
                      subjectAvatarIpfsCid: thing.subjectCroppedImageIpfsCid,
                      size: AvatarSize.medium,
                      color: Colors.white70,
                    ),
                  ],
                ),
              ),
            ),
            StatusStepperBlock(),
          ],
          leftSideBlock: EvidenceViewBlock(),
        ),
      ),
    ];

    if (state.index >= ThingStateVm.fundedAndVerifierLotteryInitiated.index) {
      items.add(Lottery(thing: thing));
      if (state.index >= ThingStateVm.verifiersSelectedAndPollInitiated.index) {
        items.add(Poll(thing: thing));
        if (state.index >= ThingStateVm.awaitingSettlement.index) {
          items.add(SettlementProposalsList(thingId: thing.id));
        }
      }
    }

    return items;
  }

  List<Widget> _buildTagChips(ThingVm thing, TextTheme textTheme) {
    return thing.tags
        .map((tag) => Padding(
              padding: const EdgeInsets.only(right: 8),
              child: Chip(
                label: Text(tag.name),
                labelStyle: textTheme.caption,
                backgroundColor: Colors.black12,
              ),
            ))
        .toList();
  }

  Widget _buildHeader(ThingVm thing) {
    var textTheme = Theme.of(context).textTheme;

    return Stack(
      children: [
        Padding(
          padding: const EdgeInsets.only(bottom: 100),
          child: ArcBannerImage(thing.imageIpfsCid!),
        ),
        Positioned(
          bottom: 10,
          left: 40,
          right: 16,
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Poster(
                thing.croppedImageIpfsCid!,
                height: 200,
              ),
              SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      thing.title,
                      style: textTheme.titleLarge,
                    ),
                    SizedBox(height: 12),
                    Row(children: _buildTagChips(thing, textTheme)),
                  ],
                ),
              ),
              SizedBox(width: 16),
              if (thing.state.index >= ThingStateVm.awaitingFunding.index)
                Column(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    Text('Submitted at'),
                    SizedBox(height: 8),
                    Text(thing.submittedAtFormatted),
                    SizedBox(height: 8),
                    Text('by ${thing.submitterIdShort}'),
                  ],
                ),
            ],
          ),
        ),
        Positioned(
          bottom: 130,
          right: 20,
          child: WatchButton(
            markedAsWatched: thing.watched,
            onPressed: (markedAsWatched) => _thingBloc.dispatch(
              Watch(
                thingId: widget.thingId,
                markedAsWatched: markedAsWatched,
              ),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildBody(GetThingRvm vm) {
    var tabs = _buildTabs(vm.thing);

    return SizedBox(
      width: double.infinity,
      height: 800,
      child: TabContainer(
        controller: TabContainerController(length: tabs.length),
        tabEdge: TabEdge.top,
        tabEnd: 0.3,
        color: Colors.blue[200],
        isStringTabs: false,
        tabs: tabs,
        children: _buildTabContents(vm),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return StreamBuilder(
      stream: _thingBloc.thing$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return SliverFillRemaining(
            hasScrollBody: false,
            child: Center(child: CircularProgressIndicator()),
          );
        }

        var vm = snapshot.data!;
        if (vm is GetThingFailureVm) {
          return SliverFillRemaining(
            hasScrollBody: false,
            child: Center(child: Text(vm.message)),
          );
        }

        vm as GetThingSuccessVm;

        return MultiSliver(
          children: [
            SliverToBoxAdapter(child: _buildHeader(vm.result.thing)),
            SliverToBoxAdapter(child: SizedBox(height: 30)),
            SliverToBoxAdapter(child: _buildBody(vm.result)),
          ],
        );
      },
    );
  }
}
