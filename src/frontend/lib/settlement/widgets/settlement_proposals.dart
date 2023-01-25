import 'package:flutter/material.dart';
import 'package:rounded_loading_button/rounded_loading_button.dart';

import '../bloc/settlement_result_vm.dart';
import 'verdict_selection_block.dart';
import '../../general/contexts/document_context.dart';
import '../../general/widgets/document_composer.dart';
import '../../general/widgets/evidence_block.dart';
import '../../general/widgets/image_block_with_crop.dart';
import '../bloc/settlement_actions.dart';
import '../bloc/settlement_bloc.dart';
import '../../widget_extensions.dart';

class SettlementProposals extends StatefulWidget {
  final String thingId;

  const SettlementProposals({super.key, required this.thingId});

  @override
  State<SettlementProposals> createState() => _SettlementProposalsState();
}

class _SettlementProposalsState extends StateX<SettlementProposals> {
  late final _settlementBloc = use<SettlementBloc>();

  @override
  void initState() {
    super.initState();
    _settlementBloc.dispatch(
      GetSettlementProposalsFor(thingId: widget.thingId),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: StreamBuilder(
        stream: _settlementBloc.proposals$,
        builder: (context, snapshot) {
          if (snapshot.data == null) {
            return Center(child: CircularProgressIndicator());
          }

          var proposals = snapshot.data!.proposals;

          return Center(
            child: Text('${proposals.length} proposals'),
          );
        },
      ),
      floatingActionButton: FloatingActionButton(
        child: Icon(Icons.add),
        onPressed: () {
          var documentContext = DocumentContext();
          documentContext.thingId = widget.thingId;
          var btnController = RoundedLoadingButtonController();

          showDialog(
            context: context,
            barrierDismissible: false,
            builder: (context) => UseScope(
              useInstances: [documentContext],
              child: DocumentComposer(
                title: 'New settlement proposal',
                nameFieldLabel: 'Title',
                submitButton: Padding(
                  padding: EdgeInsets.symmetric(horizontal: 12),
                  child: RoundedLoadingButton(
                    child: Text('Prepare draft'),
                    controller: btnController,
                    onPressed: () async {
                      var action = CreateNewSettlementProposalDraft(
                        documentContext: DocumentContext.fromEditable(
                          documentContext,
                        ),
                      );
                      _settlementBloc.dispatch(action);

                      var vm = await action.result;
                      if (vm is CreateNewSettlementProposalDraftFailureVm) {
                        btnController.error();
                        return;
                      }

                      btnController.success();
                      await Future.delayed(Duration(seconds: 2));
                      Navigator.of(context).pop();
                    },
                  ),
                ),
                sideBlocks: [
                  VerdictSelectionBlock(),
                  ImageBlockWithCrop(cropCircle: false),
                  EvidenceBlock(),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}
