import 'package:flutter/material.dart';
import 'package:auto_size_text/auto_size_text.dart';
import 'package:rounded_loading_button/rounded_loading_button.dart';

import '../../general/contexts/page_context.dart';
import '../../settlement/bloc/settlement_bloc.dart';
import '../../settlement/models/rvm/verdict_vm.dart';
import '../../general/contexts/document_context.dart';
import '../../general/widgets/document_composer.dart';
import '../../general/widgets/evidence_block.dart';
import '../../general/widgets/image_block_with_crop.dart';
import '../../settlement/bloc/settlement_actions.dart';
import '../../settlement/widgets/verdict_selection_block.dart';
import '../../subject/widgets/clipped_image.dart';
import '../../general/widgets/corner_banner.dart';
import '../bloc/thing_bloc.dart';
import '../../widget_extensions.dart';
import '../bloc/thing_actions.dart';

class SettlementProposalsList extends StatefulWidget {
  final String thingId;

  const SettlementProposalsList({super.key, required this.thingId});

  @override
  State<SettlementProposalsList> createState() =>
      _SettlementProposalsListState();
}

class _SettlementProposalsListState extends StateX<SettlementProposalsList> {
  late final _pageContext = use<PageContext>();
  late final _thingBloc = use<ThingBloc>();
  late final _settlementBloc = use<SettlementBloc>();

  @override
  void initState() {
    super.initState();
    _thingBloc.dispatch(
      GetSettlementProposalsList(thingId: widget.thingId),
    );
  }

  @override
  Widget buildX(BuildContext context) {
    return StreamBuilder(
      stream: _thingBloc.proposalsList$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return Center(child: CircularProgressIndicator());
        }

        var proposals = snapshot.data!.proposals;
        if (proposals.isEmpty) {
          return Center(
            child: IconButton(
              icon: Icon(Icons.add_box_outlined),
              onPressed: () {
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
                          child: Text('Prepare draft'),
                          controller: btnController,
                          onPressed: () async {
                            var action = CreateNewSettlementProposalDraft(
                              documentContext: DocumentContext.fromEditable(
                                documentContext,
                              ),
                            );
                            _settlementBloc.dispatch(action);

                            var failure = await action.result;
                            if (failure != null) {
                              btnController.error();
                              return;
                            }

                            btnController.success();
                            await Future.delayed(Duration(seconds: 2));
                            if (context.mounted) {
                              Navigator.of(context).pop();
                            }
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

        return Column(
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                IconButton(
                  icon: Icon(Icons.add_box_outlined),
                  onPressed: () {
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
                              child: Text('Prepare draft'),
                              controller: btnController,
                              onPressed: () async {
                                var action = CreateNewSettlementProposalDraft(
                                  documentContext: DocumentContext.fromEditable(
                                    documentContext,
                                  ),
                                );
                                _settlementBloc.dispatch(action);

                                var failure = await action.result;
                                if (failure != null) {
                                  btnController.error();
                                  return;
                                }

                                btnController.success();
                                await Future.delayed(Duration(seconds: 2));
                                if (context.mounted) {
                                  Navigator.of(context).pop();
                                }
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
              ],
            ),
            ListView.builder(
              shrinkWrap: true,
              padding: const EdgeInsets.all(16),
              itemCount: proposals.length,
              itemBuilder: (context, index) {
                var proposal = proposals[index];
                return Stack(
                  children: [
                    Card(
                      margin: EdgeInsets.zero,
                      color: Colors.blue[600],
                      elevation: 5,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Padding(
                        padding: const EdgeInsets.only(left: 250),
                        child: SizedBox(
                          width: 500,
                          height: 135,
                          child: Row(
                            children: [
                              Expanded(
                                child: Column(
                                  mainAxisAlignment: MainAxisAlignment.center,
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    AutoSizeText(
                                      proposal.title,
                                      style: TextStyle(
                                        color: Colors.white,
                                        fontSize: 18,
                                      ),
                                      maxLines: 2,
                                      overflow: TextOverflow.ellipsis,
                                    ),
                                    SizedBox(height: 12),
                                    Text(
                                      proposal.displayedTimestampFormatted,
                                      style: TextStyle(
                                        color: Colors.white70,
                                      ),
                                    ),
                                    SizedBox(height: 8),
                                    Container(
                                      color: proposal.verdictColor,
                                      width: 300,
                                      height: 35,
                                      alignment: Alignment.center,
                                      child: Text(proposal.verdict.getString()),
                                    ),
                                  ],
                                ),
                              ),
                              SizedBox(width: 12),
                              InkWell(
                                borderRadius: BorderRadius.only(
                                  topRight: Radius.circular(12),
                                  bottomRight: Radius.circular(12),
                                ),
                                child: Container(
                                  width: 42,
                                  height: double.infinity,
                                  decoration: BoxDecoration(
                                    color: Colors.grey[400],
                                    borderRadius: BorderRadius.only(
                                      topRight: Radius.circular(12),
                                      bottomRight: Radius.circular(12),
                                    ),
                                  ),
                                  alignment: Alignment.center,
                                  child: Icon(
                                    Icons.arrow_forward_ios_rounded,
                                    color: Colors.white,
                                  ),
                                ),
                                onTap: () => _pageContext.goto(
                                  '/proposals/${proposal.id}',
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
                    ),
                    ClippedImage(
                      imageIpfsCid: proposal.croppedImageIpfsCid!,
                      width: 240,
                      height: 135,
                      fromNarrowToWide: index % 2 == 1,
                    ),
                    CornerBanner(
                      position: Alignment.topLeft,
                      size: 50,
                      cornerRadius: 12,
                      color: Colors.white,
                      child: Icon(
                        proposal.stateIcon,
                        size: 22,
                      ),
                    )
                  ],
                );
              },
            ),
          ],
        );
      },
    );
  }
}
