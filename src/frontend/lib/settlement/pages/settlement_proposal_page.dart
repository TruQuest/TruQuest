import 'dart:async';

import 'package:flutter/material.dart';
import 'package:contained_tab_bar_view/contained_tab_bar_view.dart'
    show TabBarProperties, TabBarViewProperties, ContainerTabIndicator;
import 'package:google_fonts/google_fonts.dart';
import 'package:sliver_tools/sliver_tools.dart';

import '../../general/widgets/contained_tab_bar_view.dart';
import '../../user/models/vm/user_vm.dart';
import '../widgets/thing_preview_block.dart';
import '../widgets/status_stepper_block.dart';
import '../../general/widgets/arc_banner_image.dart';
import '../../general/widgets/poster.dart';
import '../../general/widgets/watch_button.dart';
import '../../user/bloc/user_bloc.dart';
import '../models/vm/get_settlement_proposal_rvm.dart';
import '../models/vm/settlement_proposal_vm.dart';
import '../widgets/lottery.dart';
import '../widgets/poll.dart';
import '../widgets/verdict_view_block.dart';
import '../models/vm/settlement_proposal_state_vm.dart';
import '../../general/contexts/document_view_context.dart';
import '../../general/widgets/document_view.dart';
import '../../general/widgets/evidence_view_block.dart';
import '../bloc/settlement_actions.dart';
import '../bloc/settlement_bloc.dart';
import '../../widget_extensions.dart';

class SettlementProposalPage extends StatefulWidget {
  final String proposalId;
  final DateTime timestamp;

  const SettlementProposalPage({
    super.key,
    required this.proposalId,
    required this.timestamp,
  });

  @override
  State<SettlementProposalPage> createState() => _SettlementProposalPageState();
}

class _SettlementProposalPageState extends StateX<SettlementProposalPage> {
  late final _userBloc = use<UserBloc>();
  late final _settlementBloc = use<SettlementBloc>();

  late final StreamSubscription<UserVm> _currentUser$$;

  @override
  void initState() {
    super.initState();
    _currentUser$$ = _userBloc.currentUser$.listen(
      (_) => _settlementBloc.dispatch(
        GetSettlementProposal(proposalId: widget.proposalId),
      ),
    );
  }

  @override
  void didUpdateWidget(covariant SettlementProposalPage oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.timestamp != oldWidget.timestamp) {
      _settlementBloc.dispatch(
        GetSettlementProposal(proposalId: widget.proposalId),
      );
    }
  }

  @override
  void dispose() {
    super.dispose();
    _currentUser$$.cancel();
  }

  List<Widget> _buildTabs(SettlementProposalVm proposal) {
    var state = proposal.state;
    var items = [const Text('Details')];

    if (state.index >= SettlementProposalStateVm.fundedAndVerifierLotteryInitiated.index) {
      items.add(const Text('Lottery'));
      if (state.index >= SettlementProposalStateVm.verifiersSelectedAndPollInitiated.index) {
        items.add(const Text('Poll'));
      }
    }

    return items;
  }

  List<Widget> _buildTabContents(GetSettlementProposalRvm vm) {
    var proposal = vm.proposal;
    var state = proposal.state;

    var items = <Widget>[
      ScopeX(
        updatesShouldNotify: true,
        useInstances: [
          DocumentViewContext(
            nameOrTitle: proposal.title,
            details: proposal.details,
            proposal: proposal,
            evidence: proposal.evidence,
            signature: vm.signature,
          ),
        ],
        child: DocumentView(
          rightSideBlocks: [
            ThingPreviewBlock(),
            VerdictViewBlock(),
            StatusStepperBlock(),
          ],
          leftSideBlock: const EvidenceViewBlock(),
        ),
      ),
    ];

    if (state.index >= SettlementProposalStateVm.fundedAndVerifierLotteryInitiated.index) {
      items.add(Lottery(proposal: proposal));
      if (state.index >= SettlementProposalStateVm.verifiersSelectedAndPollInitiated.index) {
        items.add(Poll(proposal: proposal));
      }
    }

    return items;
  }

  Widget _buildHeader(SettlementProposalVm proposal) {
    return Stack(
      children: [
        Padding(
          padding: const EdgeInsets.only(bottom: 100),
          child: ArcBannerImage(proposal.imageIpfsCid!),
        ),
        Positioned(
          bottom: 10,
          left: 40,
          right: 16,
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Poster(
                proposal.croppedImageIpfsCid!,
                height: 200,
              ),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Padding(
                      padding: const EdgeInsets.only(bottom: 32),
                      child: Text(
                        proposal.title,
                        style: GoogleFonts.philosopher(fontSize: 31),
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 16),
              if (proposal.state.index >= SettlementProposalStateVm.awaitingFunding.index)
                Column(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    Text(
                      'Submitted on',
                      style: GoogleFonts.raleway(),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      proposal.submittedAtFormatted,
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
                            text: proposal.submitterWalletAddressShort,
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
            markedAsWatched: proposal.watched,
            onPressed: (markedAsWatched) {}, // @@TODO
          ),
        ),
      ],
    );
  }

  Widget _buildBody(GetSettlementProposalRvm vm) {
    var tabs = _buildTabs(vm.proposal);

    return SizedBox(
      width: double.infinity,
      height: 800,
      child: Stack(
        children: [
          ContainedTabBarView(
            key: ValueKey('${vm.proposal.id} ${vm.proposal.state}'), // @@TODO: Check if necessary.
            tabs: tabs,
            tabBarProperties: TabBarProperties(
              margin: const EdgeInsets.only(bottom: 8),
              width: tabs.length == 1 ? 300 : 600,
              height: 40,
              indicator: ContainerTabIndicator(
                radius: BorderRadius.circular(8),
                color: Colors.indigo,
              ),
              labelColor: Colors.white,
              unselectedLabelColor: Colors.grey,
            ),
            tabBarViewProperties: const TabBarViewProperties(
              physics: NeverScrollableScrollPhysics(),
            ),
            views: _buildTabContents(vm),
          ),
          if (vm.proposal.state.index >= SettlementProposalStateVm.declined.index)
            Positioned(
              top: 30,
              left: 24,
              child: Card(
                margin: EdgeInsets.zero,
                color: Colors.redAccent,
                child: Padding(
                  padding: const EdgeInsets.fromLTRB(16, 8, 16, 8),
                  child: Text(
                    vm.proposal.state == SettlementProposalStateVm.accepted ? 'Accepted' : 'Declined',
                    style: GoogleFonts.righteous(
                      color: Colors.white,
                      fontSize: 20,
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
      stream: _settlementBloc.proposal$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return const SliverFillRemaining(
            hasScrollBody: false,
            child: Center(child: CircularProgressIndicator()),
          );
        }

        var vm = snapshot.data!;

        return MultiSliver(
          children: [
            SliverToBoxAdapter(child: _buildHeader(vm.proposal)),
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.only(top: 30),
                child: _buildBody(vm),
              ),
            ),
          ],
        );
      },
    );
  }
}
