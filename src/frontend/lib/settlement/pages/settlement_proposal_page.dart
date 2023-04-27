import 'package:flutter/material.dart';
import 'package:tab_container/tab_container.dart';

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
      GetSettlementProposal(
        proposalId: widget.proposalId,
        subscribe: true,
      ),
    );
  }

  @override
  void dispose() {
    _settlementBloc.dispatch(
      UnsubscribeFromProposal(proposalId: widget.proposalId),
    );
    super.dispose();
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

  List<Widget> _buildContent(GetSettlementProposalRvm vm) {
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
          sideBlocks: [
            VerdictViewBlock(),
            TimelineBlock(),
          ],
          bottomBlock: EvidenceViewBlock(),
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

  @override
  Widget build(BuildContext context) {
    return StreamBuilder(
      stream: _settlementBloc.proposal$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return Center(child: CircularProgressIndicator());
        }

        var vm = snapshot.data!;
        var tabs = _buildTabs(vm.proposal);

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
