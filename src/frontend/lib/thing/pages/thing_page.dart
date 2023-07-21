import 'dart:async';

import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:rounded_loading_button/rounded_loading_button.dart';
import 'package:sliver_tools/sliver_tools.dart';

import '../../general/widgets/restrict_when_unauthorized_button.dart';
import '../../general/contexts/document_context.dart';
import '../../general/contexts/page_context.dart';
import '../../general/widgets/document_composer.dart';
import '../../general/widgets/evidence_block.dart';
import '../../general/widgets/image_block_with_crop.dart';
import '../../general/widgets/tab_container.dart';
import '../../settlement/bloc/settlement_actions.dart';
import '../../settlement/bloc/settlement_bloc.dart';
import '../../settlement/widgets/verdict_selection_block.dart';
import '../../user/models/vm/user_vm.dart';
import '../widgets/status_stepper_block.dart';
import '../bloc/thing_result_vm.dart';
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
  final DateTime timestamp;

  const ThingPage({
    super.key,
    required this.thingId,
    required this.timestamp,
  });

  @override
  State<ThingPage> createState() => _ThingPageState();
}

class _ThingPageState extends StateX<ThingPage> {
  late final _pageContext = use<PageContext>();
  late final _userBloc = use<UserBloc>();
  late final _thingBloc = use<ThingBloc>();
  // late final _settlementBloc = use<SettlementBloc>();

  late final StreamSubscription<UserVm> _currentUser$$;

  final List<Color> _tabColors = const [
    Color(0xFF242423),
    Color(0xFF413C69),
    Color(0xFF32407B),
    Color(0xFF0F6292),
  ];

  @override
  void initState() {
    super.initState();
    _currentUser$$ = _userBloc.currentUser$.listen(
      (_) => _thingBloc.dispatch(GetThing(thingId: widget.thingId)),
    );
  }

  @override
  void didUpdateWidget(covariant ThingPage oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.timestamp != oldWidget.timestamp) {
      _thingBloc.dispatch(GetThing(thingId: widget.thingId));
    }
  }

  @override
  void dispose() {
    super.dispose();
    _currentUser$$.cancel();
  }

  List<Widget> _buildTabs(ThingVm thing) {
    var state = thing.state;
    var items = [
      const Icon(
        Icons.content_paste,
        color: Colors.white,
      )
    ];

    if (state.index >= ThingStateVm.fundedAndVerifierLotteryInitiated.index) {
      items.add(const Icon(
        Icons.people,
        color: Colors.white,
      ));
      if (state.index >= ThingStateVm.verifiersSelectedAndPollInitiated.index) {
        items.add(const Icon(
          Icons.poll_outlined,
          color: Colors.white,
        ));
        if (state.index >= ThingStateVm.awaitingSettlement.index) {
          items.add(const Icon(
            Icons.handshake,
            color: Colors.white,
          ));
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
            thing: thing,
            evidence: thing.evidence,
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
                    const SizedBox(height: 10),
                    InkWell(
                      onTap: () => _pageContext.goto(
                        '/subjects/${thing.subjectId}',
                      ),
                      child: AvatarWithReputationGauge(
                        subjectId: thing.subjectId,
                        subjectAvatarIpfsCid: thing.subjectCroppedImageIpfsCid,
                        value: thing.subjectAvgScore.toDouble(),
                        size: AvatarSize.medium,
                        color: Colors.white,
                      ),
                    ),
                    const SizedBox(height: 20),
                    Container(
                      decoration: const BoxDecoration(
                        border: Border(
                          bottom: BorderSide(color: Colors.white),
                        ),
                      ),
                      padding: const EdgeInsets.fromLTRB(4, 0, 4, 6),
                      child: Text(
                        thing.subjectName,
                        style: GoogleFonts.philosopher(
                          color: Colors.white,
                          fontSize: 28,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
            StatusStepperBlock(),
          ],
          leftSideBlock: const EvidenceViewBlock(),
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

  List<Widget> _buildTagChips(ThingVm thing) {
    return thing.tags
        .map((tag) => Padding(
              padding: const EdgeInsets.only(right: 8),
              child: Chip(
                label: Text(tag.name),
                labelStyle: GoogleFonts.righteous(
                  color: const Color(0xffF8F9FA),
                ),
                backgroundColor: const Color(0xFF242423),
              ),
            ))
        .toList();
  }

  Widget _buildHeader(ThingVm thing) {
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
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      thing.title,
                      style: GoogleFonts.philosopher(fontSize: 31),
                    ),
                    const SizedBox(height: 12),
                    Row(children: _buildTagChips(thing)),
                  ],
                ),
              ),
              const SizedBox(width: 16),
              if (thing.state.index >= ThingStateVm.awaitingFunding.index)
                Column(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    Text(
                      'Submitted on',
                      style: GoogleFonts.raleway(),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      thing.submittedAtFormatted,
                      style: GoogleFonts.raleway(
                        fontSize: 16,
                      ),
                    ),
                    const SizedBox(height: 8),
                    RichText(
                      text: TextSpan(
                        children: [
                          TextSpan(
                            text: 'by ',
                            style: GoogleFonts.raleway(),
                          ),
                          TextSpan(
                            text: thing.submitterIdShort,
                            style: GoogleFonts.raleway(
                              fontSize: 16,
                            ),
                          ),
                        ],
                      ),
                    ),
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
      child: Stack(
        children: [
          TabContainer(
            key: ValueKey('${vm.thing.id} ${vm.thing.state}'),
            controller: TabContainerController(length: tabs.length),
            tabEdge: TabEdge.top,
            tabStart: 0.33,
            tabEnd: 0.66,
            colors: _tabColors.sublist(0, tabs.length),
            isStringTabs: false,
            tabs: tabs,
            children: _buildTabContents(vm),
          ),
          if (vm.thing.state == ThingStateVm.consensusNotReached ||
              vm.thing.state == ThingStateVm.verifierLotteryFailed)
            Positioned(
              top: 30,
              left: 24,
              child: Tooltip(
                message: vm.thing.state == ThingStateVm.consensusNotReached
                    ? 'Consensus not reached'
                    : 'Not enough lottery participants',
                child: InkWell(
                  onTap: () => _pageContext.goto(
                    '/things/${vm.thing.relatedThings!['next']}',
                  ),
                  child: Card(
                    margin: EdgeInsets.zero,
                    color: Colors.redAccent,
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(16, 8, 16, 8),
                      child: Text(
                        'Archived',
                        style: GoogleFonts.righteous(
                          color: Colors.white,
                          fontSize: 20,
                        ),
                      ),
                    ),
                  ),
                ),
              ),
            ),
          if (vm.thing.state == ThingStateVm.awaitingSettlement)
            Positioned(
              top: 30,
              left: 24,
              child: RestrictWhenUnauthorizedButton(
                child: InkWell(
                  onTap: () {
                    var documentContext = DocumentContext();
                    documentContext.thingId = widget.thingId;

                    var btnController = RoundedLoadingButtonController();

                    showDialog(
                      context: context,
                      barrierDismissible: false,
                      builder: (context) => ScopeX(
                        useInstances: [documentContext],
                        child: DocumentComposer(
                          title: 'New settlement proposal',
                          nameFieldLabel: 'Title',
                          submitButton: Padding(
                            padding: const EdgeInsets.symmetric(horizontal: 12),
                            child: RoundedLoadingButton(
                              child: const Text('Prepare draft'),
                              controller: btnController,
                              onPressed: () async {
                                var action = CreateNewSettlementProposalDraft(
                                  documentContext: DocumentContext.fromEditable(
                                    documentContext,
                                  ),
                                );
                                // _settlementBloc.dispatch(action);

                                var failure = await action.result;
                                if (failure != null) {
                                  btnController.error();
                                  await Future.delayed(
                                    const Duration(milliseconds: 1500),
                                  );
                                  btnController.reset();

                                  return;
                                }

                                btnController.success();
                                await Future.delayed(
                                  const Duration(milliseconds: 1500),
                                );
                                if (context.mounted) {
                                  Navigator.of(context).pop();
                                }
                              },
                            ),
                          ),
                          sideBlocks: const [
                            VerdictSelectionBlock(),
                            ImageBlockWithCrop(cropCircle: false),
                            EvidenceBlock(),
                          ],
                        ),
                      ),
                    );
                  },
                  child: Card(
                    margin: EdgeInsets.zero,
                    color: Colors.redAccent,
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(16, 8, 16, 8),
                      child: Text(
                        'Awaiting settlement',
                        style: GoogleFonts.righteous(
                          color: Colors.white,
                          fontSize: 20,
                        ),
                      ),
                    ),
                  ),
                ),
              ),
            ),
          if (vm.thing.state == ThingStateVm.declined)
            Positioned(
              top: 30,
              left: 24,
              child: Card(
                margin: EdgeInsets.zero,
                color: Colors.redAccent,
                child: Padding(
                  padding: const EdgeInsets.fromLTRB(16, 8, 16, 8),
                  child: Text(
                    'Declined',
                    style: GoogleFonts.righteous(
                      color: Colors.white,
                      fontSize: 20,
                    ),
                  ),
                ),
              ),
            ),
          if (vm.thing.acceptedSettlementProposalId != null)
            Positioned(
              top: 30,
              left: 24,
              child: InkWell(
                onTap: () => _pageContext.goto(
                  '/proposals/${vm.thing.acceptedSettlementProposalId}',
                ),
                child: Card(
                  margin: EdgeInsets.zero,
                  color: Colors.redAccent,
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(16, 8, 16, 8),
                    child: Text(
                      'Settled',
                      style: GoogleFonts.righteous(
                        color: Colors.white,
                        fontSize: 20,
                      ),
                    ),
                  ),
                ),
              ),
            ),
        ],
      ),
    );
  }

  @override
  Widget buildX(BuildContext context) {
    return StreamBuilder(
      stream: _thingBloc.thing$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return const SliverFillRemaining(
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
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.only(top: 30),
                child: _buildBody(vm.result),
              ),
            ),
          ],
        );
      },
    );
  }
}
