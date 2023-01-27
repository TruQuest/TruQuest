import 'package:flutter/material.dart';
import 'package:tabbed_view/tabbed_view.dart';

import '../widgets/lottery.dart';
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

  @override
  Widget build(BuildContext context) {
    return StreamBuilder(
      stream: _settlementBloc.proposal$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return Center(child: CircularProgressIndicator());
        }

        var vm = snapshot.data!;
        var proposal = vm.proposal;

        var documentViewContext = DocumentViewContext(
          nameOrTitle: proposal.title,
          details: proposal.details,
          proposal: proposal,
          signature: vm.signature,
        );

        // @@!!: Not being disposed of!
        var controller = TabbedViewController(
          [
            TabData(
              text: 'Details',
              content: UseScope(
                useInstances: [documentViewContext],
                preserveOnRebuild: false,
                child: DocumentView(
                  sideBlocks: [
                    VerdictViewBlock(),
                    TimelineBlock(),
                  ],
                  bottomBlock: EvidenceViewBlock(),
                ),
              ),
              closable: false,
              buttons: [
                TabButton(
                  icon: IconProvider.data(Icons.refresh),
                  onPressed: () {
                    _settlementBloc.dispatch(
                      GetSettlementProposal(proposalId: widget.proposalId),
                    );
                  },
                ),
              ],
            ),
            if (proposal.state.index >=
                SettlementProposalStateVm
                    .fundedAndAssessmentVerifierLotteryInitiated.index)
              TabData(
                text: 'Verifier Lottery',
                content: Lottery(proposal: proposal),
                closable: false,
              ),
            // if (proposal.state.index >=
            //     SettlementProposalStateVm.assessmentVerifiersSelectedAndPollInitiated.index)
            //   TabData(
            //     text: 'Assessment Poll',
            //     content: Poll(thing: proposal),
            //     closable: false,
            //   ),
          ],
        );

        return TabbedView(controller: controller);
      },
    );
  }
}
