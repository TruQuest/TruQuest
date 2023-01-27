import 'package:flutter/material.dart';
import 'package:rounded_loading_button/rounded_loading_button.dart';
import 'package:timelines/timelines.dart';

import '../../general/contexts/document_view_context.dart';
import '../bloc/settlement_actions.dart';
import '../bloc/settlement_bloc.dart';
import '../models/rvm/settlement_proposal_state_vm.dart';
import '../../widget_extensions.dart';

class TimelineBlock extends StatefulWidget {
  const TimelineBlock({super.key});

  @override
  State<TimelineBlock> createState() => _TimelineBlockState();
}

class _TimelineBlockState extends StateX<TimelineBlock> {
  late final _documentViewContext = useScoped<DocumentViewContext>();
  late final _settlementBloc = use<SettlementBloc>();

  Widget _buildDraftTile() {
    var proposal = _documentViewContext.proposal!;
    var btnController = RoundedLoadingButtonController();

    return RoundedLoadingButton(
      controller: btnController,
      onPressed: // draft proposal should only be available to its creator
          proposal.state == SettlementProposalStateVm.draft
              ? () async {
                  print('Edit draft');

                  await Future.delayed(Duration(seconds: 2));
                  btnController.success();
                }
              : null,
      child: Text('Draft'),
    );
  }

  Widget _buildDraftTileIndicator() {
    var state = _documentViewContext.proposal!.state;
    return state == SettlementProposalStateVm.draft
        ? DotIndicator()
        : OutlinedDotIndicator();
  }

  Widget _buildSubmitTile() {
    var proposal = _documentViewContext.proposal!;
    var btnController = RoundedLoadingButtonController();
    var ignoreClick = false; // @@!!: Think of a better way!

    return RoundedLoadingButton(
      controller: btnController,
      onPressed: // draft proposal should only be available to its creator
          proposal.state == SettlementProposalStateVm.draft
              ? () async {
                  if (ignoreClick) return;

                  var action = SubmitNewSettlementProposal(
                    proposalId: proposal.id,
                  );
                  _settlementBloc.dispatch(action);

                  var vm = await action.result;
                  if (vm == null) {
                    btnController.error();
                    await Future.delayed(Duration(seconds: 2));
                    btnController.reset();
                    return;
                  }

                  btnController.success();
                  await Future.delayed(Duration(seconds: 1));

                  ignoreClick = true;
                  btnController.reset();
                  await Future.delayed(Duration(seconds: 1));

                  _settlementBloc.dispatch(
                    GetSettlementProposal(proposalId: proposal.id),
                  );
                }
              : null,
      child: Text('Submit'),
    );
  }

  Widget _buildFundTile() {
    var proposal = _documentViewContext.proposal!;
    bool canBeFunded =
        proposal.state == SettlementProposalStateVm.awaitingFunding &&
            proposal.canBeFunded!; // && isCreator
    var btnController = RoundedLoadingButtonController();
    var ignoreClick = false;

    return RoundedLoadingButton(
      controller: btnController,
      onPressed: canBeFunded
          ? () async {
              if (ignoreClick) return;
              var action = FundSettlementProposal(
                thingId: proposal.thingId,
                proposalId: proposal.id,
                signature: _documentViewContext.signature!,
              );
              _settlementBloc.dispatch(action);

              var vm = await action.result;
              if (vm == null) {
                btnController.error();
                await Future.delayed(Duration(seconds: 2));
                btnController.reset();
                return;
              }

              btnController.success();
              await Future.delayed(Duration(seconds: 1));

              ignoreClick = true;
              btnController.reset();
              await Future.delayed(Duration(seconds: 1));

              _settlementBloc.dispatch(
                GetSettlementProposal(proposalId: proposal.id),
              );
            }
          : null,
      child: Text('Fund'),
    );
  }

  Widget _buildFundTileIndicator() {
    var proposal = _documentViewContext.proposal!;
    bool canBeFunded =
        proposal.state == SettlementProposalStateVm.awaitingFunding &&
            proposal.canBeFunded!; // && isCreator

    return canBeFunded ? DotIndicator() : OutlinedDotIndicator();
  }

  Widget _buildLotteryTile() {
    var state = _documentViewContext.proposal!.state;
    return RoundedLoadingButton(
      controller: RoundedLoadingButtonController(),
      animateOnTap: false,
      child: Text('Verifier lottery\nin progress'),
      onPressed: state.index >=
              SettlementProposalStateVm
                  .fundedAndAssessmentVerifierLotteryInitiated.index
          ? () {}
          : null,
    );
  }

  Widget _buildLotteryTileIndicator() {
    var state = _documentViewContext.proposal!.state;
    return state ==
            SettlementProposalStateVm
                .fundedAndAssessmentVerifierLotteryInitiated
        ? DotIndicator(
            size: 15,
            child: Padding(
              padding: EdgeInsets.all(2),
              child: CircularProgressIndicator(color: Colors.white),
            ),
          )
        : OutlinedDotIndicator();
  }

  // Widget _buildPollTile() {
  //   var state = _documentViewContext.proposal!.state;
  //   return InkWell(
  //     onTap: state ==
  //             SettlementProposalStateVm
  //                 .assessmentVerifiersSelectedAndPollInitiated
  //         ? () {}
  //         : null,
  //     child: Card(
  //       elevation: state ==
  //               SettlementProposalStateVm
  //                   .assessmentVerifiersSelectedAndPollInitiated
  //           ? 5
  //           : 0,
  //       child: Padding(
  //         padding: EdgeInsets.all(8),
  //         child: Text('Assessment poll\nin progress'),
  //       ),
  //     ),
  //   );
  // }

  // Widget _buildPollTileIndicator() {
  //   var state = _documentViewContext.proposal!.state;
  //   return state ==
  //           SettlementProposalStateVm
  //               .assessmentVerifiersSelectedAndPollInitiated
  //       ? DotIndicator(child: CircularProgressIndicator(color: Colors.white))
  //       : OutlinedDotIndicator();
  // }

  TimelineTileBuilder _buildTimeline() {
    // Draft -> Submit -> Fund -> Verifier lottery in progress -> Assessment poll in progress -> Assessment
    return TimelineTileBuilder.connected(
      itemCount: 4,
      contentsAlign: ContentsAlign.basic,
      connectionDirection: ConnectionDirection.after,
      indicatorBuilder: (context, index) {
        switch (index) {
          case 0:
            return _buildDraftTileIndicator();
          case 1:
            return _buildDraftTileIndicator();
          case 2:
            return _buildFundTileIndicator();
          case 3:
            return _buildLotteryTileIndicator();
          // case 4:
          //   return _buildPollTileIndicator();
        }

        throw UnimplementedError();
      },
      contentsBuilder: (context, index) {
        Widget? child;
        switch (index) {
          case 0:
            child = _buildDraftTile();
            break;
          case 1:
            child = _buildSubmitTile();
            break;
          case 2:
            child = _buildFundTile();
            break;
          case 3:
            child = _buildLotteryTile();
          // case 4:
          //   return _buildPollTile();
        }

        return Padding(
          padding: EdgeInsets.symmetric(horizontal: 24, vertical: 4),
          child: child,
        );
      },
      connectorBuilder: (context, index, type) => SolidLineConnector(),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Card(
          color: Colors.blue[600],
          elevation: 5,
          child: Container(
            width: double.infinity,
            height: 30,
            alignment: Alignment.center,
            child: Text(
              'Timeline',
              style: TextStyle(color: Colors.white),
            ),
          ),
        ),
        SizedBox(height: 6),
        SizedBox(
          width: double.infinity,
          height: 400,
          child: FixedTimeline.tileBuilder(
            theme: TimelineThemeData(
              nodePosition: 0,
            ),
            builder: _buildTimeline(),
          ),
        ),
      ],
    );
  }
}
