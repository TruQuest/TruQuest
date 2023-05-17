import 'package:flutter/material.dart';
import 'package:tab_container/tab_container.dart';

import '../../general/widgets/arc_banner_image.dart';
import '../../general/widgets/poster.dart';
import '../models/rvm/get_settlement_proposal_rvm.dart';
import '../models/rvm/settlement_proposal_vm.dart';
import '../widgets/lottery.dart';
import '../widgets/poll.dart';
import '../widgets/verdict_view_block.dart';
import '../widgets/timeline_block.dart';
import '../models/rvm/settlement_proposal_state_vm.dart';
import '../../general/contexts/document_view_context.dart';
import '../../general/widgets/document_view.dart';
import '../../general/widgets/evidence_view_block.dart';
import '../bloc/settlement_actions.dart';
import '../bloc/settlement_bloc.dart';
import '../../widget_extensions.dart';

class SettlementProposalPage extends StatefulWidget {
  final String proposalId;

  const SettlementProposalPage({super.key, required this.proposalId});

  @override
  State<SettlementProposalPage> createState() => _SettlementProposalPageState();
}

class _SettlementProposalPageState extends StateX<SettlementProposalPage> {
  late final _settlementBloc = use<SettlementBloc>();

  @override
  void initState() {
    super.initState();
    _settlementBloc.dispatch(
      GetSettlementProposal(proposalId: widget.proposalId),
    );
  }

  List<Widget> _buildTabs(SettlementProposalVm proposal) {
    var state = proposal.state;
    var items = [Icon(Icons.content_paste)];

    if (state.index >=
        SettlementProposalStateVm.fundedAndVerifierLotteryInitiated.index) {
      items.add(Icon(Icons.people));
      if (state.index >=
          SettlementProposalStateVm.verifiersSelectedAndPollInitiated.index) {
        items.add(Icon(Icons.poll_outlined));
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
            signature: vm.signature,
          ),
        ],
        child: DocumentView(
          rightSideBlocks: [
            VerdictViewBlock(),
            TimelineBlock(),
          ],
          leftSideBlock: EvidenceViewBlock(),
        ),
      ),
    ];

    if (state.index >=
        SettlementProposalStateVm.fundedAndVerifierLotteryInitiated.index) {
      items.add(Lottery(proposal: proposal));
      if (state.index >=
          SettlementProposalStateVm.verifiersSelectedAndPollInitiated.index) {
        items.add(Poll(proposal: proposal));
      }
    }

    return items;
  }

  Widget _buildHeader(SettlementProposalVm proposal) {
    var textTheme = Theme.of(context).textTheme;

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
              SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      proposal.title,
                      style: textTheme.titleLarge,
                    ),
                  ],
                ),
              ),
              SizedBox(width: 16),
              if (proposal.state.index >=
                  SettlementProposalStateVm.awaitingFunding.index)
                Column(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    Text('Submitted at'),
                    SizedBox(height: 8),
                    Text(proposal.submittedAtFormatted),
                    SizedBox(height: 8),
                    Text('by ${proposal.submitterIdShort}'),
                  ],
                ),
            ],
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
      stream: _settlementBloc.proposal$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return Center(child: CircularProgressIndicator());
        }

        var vm = snapshot.data!;

        return SingleChildScrollView(
          child: Column(
            children: [
              _buildHeader(vm.proposal),
              SizedBox(height: 30),
              _buildBody(vm),
            ],
          ),
        );
      },
    );
  }
}
