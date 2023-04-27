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
  late DocumentViewContext _documentViewContext;
  late final _settlementBloc = use<SettlementBloc>();

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    _documentViewContext = useScoped<DocumentViewContext>();
  }

  Widget _buildDraftTile() {
    var proposal = _documentViewContext.proposal!;
    var btnController = RoundedLoadingButtonController();

    return RoundedLoadingButton(
      controller: btnController,
      onPressed: proposal.state == SettlementProposalStateVm.draft
          ? () async {
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
    var ignoreClick = false; // @@TODO!!: Think of a better way!

    return RoundedLoadingButton(
      controller: btnController,
      onPressed: proposal.state == SettlementProposalStateVm.draft
          ? () async {
              if (ignoreClick) return;

              var action = SubmitNewSettlementProposal(
                proposalId: proposal.id,
              );
              _settlementBloc.dispatch(action);

              var success = await action.result;
              if (success == null) {
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
            proposal.canBeFunded!; // @@TODO: && isCreator
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

              var success = await action.result;
              if (success == null) {
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
            proposal.canBeFunded!; // @@TODO: && isCreator

    return canBeFunded ? DotIndicator() : OutlinedDotIndicator();
  }

  Widget _buildLotteryTile() {
    return RoundedLoadingButton(
      controller: RoundedLoadingButtonController(),
      disabledColor: Colors.lightBlue,
      child: Text('Verifier lottery\nin progress'),
      onPressed: null,
    );
  }

  Widget _buildLotteryTileIndicator() {
    var state = _documentViewContext.proposal!.state;
    return state == SettlementProposalStateVm.fundedAndVerifierLotteryInitiated
        ? SizedBox.square(
            dimension: 15,
            child: CircularProgressIndicator(strokeWidth: 2),
          )
        : OutlinedDotIndicator();
  }

  Widget _buildPollTile() {
    return RoundedLoadingButton(
      controller: RoundedLoadingButtonController(),
      disabledColor: Colors.lightBlue,
      child: Text('Assessment poll\nin progress'),
      onPressed: null,
    );
  }

  Widget _buildPollTileIndicator() {
    var state = _documentViewContext.proposal!.state;
    return state == SettlementProposalStateVm.verifiersSelectedAndPollInitiated
        ? SizedBox.square(
            dimension: 15,
            child: CircularProgressIndicator(strokeWidth: 2),
          )
        : OutlinedDotIndicator();
  }

  TimelineTileBuilder _buildTimeline() {
    return TimelineTileBuilder.connected(
      itemCount: 5,
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
          case 4:
            return _buildPollTileIndicator();
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
            break;
          case 4:
            child = _buildPollTile();
            break;
        }

        return Padding(
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 4),
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
