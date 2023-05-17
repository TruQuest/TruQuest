import 'package:flutter/material.dart';
import 'package:auto_size_text/auto_size_text.dart';

import '../../general/contexts/page_context.dart';
import '../../general/contexts/document_context.dart';
import '../../general/widgets/document_composer.dart';
import '../../general/widgets/evidence_block.dart';
import '../../general/widgets/image_block_with_crop.dart';
import '../../general/widgets/prepare_draft_button.dart';
import '../../general/widgets/tags_block.dart';
import '../../general/widgets/corner_banner.dart';
import '../bloc/subject_actions.dart';
import '../bloc/subject_bloc.dart';
import '../../widget_extensions.dart';
import 'clipped_image.dart';

class ThingsList extends StatefulWidget {
  final String subjectId;

  const ThingsList({super.key, required this.subjectId});

  @override
  State<ThingsList> createState() => _ThingsListState();
}

class _ThingsListState extends StateX<ThingsList> {
  late final _pageContext = use<PageContext>();
  late final _subjectBloc = use<SubjectBloc>();

  @override
  void initState() {
    super.initState();
    _subjectBloc.dispatch(GetThingsList(subjectId: widget.subjectId));
  }

  @override
  Widget build(BuildContext context) {
    return StreamBuilder(
      stream: _subjectBloc.thingsList$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return Center(child: CircularProgressIndicator());
        }

        var things = snapshot.data!.things;
        if (things.isEmpty) {
          return Center(
            child: IconButton(
              icon: Icon(Icons.add_box_outlined),
              onPressed: () {
                var documentContext = DocumentContext();
                documentContext.subjectId = widget.subjectId;

                showDialog(
                  context: context,
                  barrierDismissible: false,
                  builder: (_) => ScopeX(
                    useInstances: [documentContext],
                    child: DocumentComposer(
                      title: 'New thing',
                      nameFieldLabel: 'Title',
                      submitButton: PrepareDraftButton(),
                      sideBlocks: [
                        ImageBlockWithCrop(cropCircle: false),
                        TagsBlock(),
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
                    documentContext.subjectId = widget.subjectId;

                    showDialog(
                      context: context,
                      barrierDismissible: false,
                      builder: (_) => ScopeX(
                        useInstances: [documentContext],
                        child: DocumentComposer(
                          title: 'New thing',
                          nameFieldLabel: 'Title',
                          submitButton: PrepareDraftButton(),
                          sideBlocks: [
                            ImageBlockWithCrop(cropCircle: false),
                            TagsBlock(),
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
              itemCount: things.length,
              itemBuilder: (context, index) {
                var thing = things[index];
                return Stack(
                  children: [
                    Card(
                      margin: const EdgeInsets.symmetric(vertical: 8),
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
                                      thing.title,
                                      style: TextStyle(
                                        color: Colors.white,
                                        fontSize: 18,
                                      ),
                                      maxLines: 2,
                                      overflow: TextOverflow.ellipsis,
                                    ),
                                    SizedBox(height: 12),
                                    Text(
                                      thing.displayedTimestampFormatted,
                                      style: TextStyle(
                                        color: Colors.white70,
                                      ),
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
                                  '/things/${thing.id}',
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
                    ),
                    ClippedImage(
                      imageIpfsCid: thing.croppedImageIpfsCid!,
                      width: 240,
                      height: 135,
                      fromNarrowToWide: index % 2 == 1,
                    ),
                    CornerBanner(
                      position: Alignment.topLeft,
                      size: 50,
                      cornerRadius: 12,
                      color: thing.verdict != null
                          ? thing.verdictColor
                          : Colors.grey[400]!,
                      child: Icon(
                        thing.stateIcon,
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
