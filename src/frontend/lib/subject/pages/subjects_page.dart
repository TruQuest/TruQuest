import 'package:flutter/material.dart';
import 'package:auto_size_text/auto_size_text.dart';

import '../widgets/clipped_avatar_container.dart';
import '../widgets/avatar_with_reputation_gauge.dart';
import '../widgets/corner_banner.dart';
import '../../general/contexts/page_context.dart';
import '../bloc/subject_actions.dart';
import '../bloc/subject_bloc.dart';
import '../widgets/submit_button.dart';
import '../widgets/type_selector_block.dart';
import '../../general/widgets/document_composer.dart';
import '../../general/widgets/image_block_with_crop.dart';
import '../../general/widgets/tags_block.dart';
import '../../widget_extensions.dart';

class SubjectsPage extends StatefulWidget {
  SubjectsPage({super.key});

  @override
  State<SubjectsPage> createState() => _SubjectsPageState();
}

class _SubjectsPageState extends StateX<SubjectsPage> {
  late final _pageContext = useScoped<PageContext>();
  late final _subjectBloc = use<SubjectBloc>();

  @override
  void initState() {
    super.initState();
    _subjectBloc.dispatch(GetSubjects());
  }

  @override
  Widget build(BuildContext context) {
    return StreamBuilder(
      stream: _subjectBloc.subjects$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return Center(
            child: CircularProgressIndicator(),
          );
        }

        var subjects = snapshot.data!;
        if (subjects.isEmpty) {
          return Center(
            child: Text('Nothing here yet'),
          );
        }

        return ListView.builder(
          padding: const EdgeInsets.all(16),
          itemCount: subjects.length,
          itemBuilder: (context, index) {
            var subject = subjects[index];
            return Padding(
              padding: const EdgeInsets.symmetric(vertical: 8),
              child: Stack(
                children: [
                  Card(
                    margin: EdgeInsets.zero,
                    color: Colors.white,
                    elevation: 15,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Padding(
                      padding: const EdgeInsets.only(left: 250),
                      child: SizedBox(
                        width: 500,
                        height: 200,
                        child: Row(
                          children: [
                            Expanded(
                              child: Column(
                                mainAxisAlignment: MainAxisAlignment.center,
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  AutoSizeText(
                                    subject.name,
                                    style: TextStyle(
                                      color: Colors.black,
                                      fontSize: 18,
                                    ),
                                    maxLines: 2,
                                    overflow: TextOverflow.ellipsis,
                                  ),
                                  SizedBox(height: 12),
                                  Text(
                                    subject.submittedAtFormatted,
                                    style: TextStyle(
                                      color: Colors.black87,
                                    ),
                                  ),
                                  SizedBox(height: 12),
                                  Wrap(
                                    spacing: 8,
                                    runSpacing: 6,
                                    children: subject.tags
                                        .map(
                                          (tag) => Chip(
                                            backgroundColor: Colors.grey,
                                            labelPadding:
                                                const EdgeInsets.symmetric(
                                              horizontal: 4,
                                            ),
                                            label: Text(tag.name),
                                            labelStyle: TextStyle(
                                              color: Colors.white,
                                              fontSize: 12,
                                            ),
                                          ),
                                        )
                                        .toList(),
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
                              onTap: () {},
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                  ClippedAvatarContainer(
                    child: AvatarWithReputationGauge(
                      subjectId: subject.id,
                      subjectAvatarIpfsCid: subject.croppedImageIpfsCid,
                      size: AvatarSize.small,
                      color: Colors.white,
                    ),
                    fromNarrowToWide: index % 2 == 0,
                  ),
                  CornerBanner(
                    position: Alignment.topLeft,
                    size: 50,
                    cornerRadius: 12,
                    color: Colors.white,
                    icon: subject.typeIcon,
                    iconSize: 18,
                  )
                ],
              ),
            );
          },
        );
      },
    );
  }

  // @override
  // Widget build(BuildContext context) {
  //   return Scaffold(
  //     body: Center(child: Text('Subjects')),
  //     floatingActionButton: FloatingActionButton(
  //       child: Icon(Icons.add),
  //       onPressed: () async {
  //         var jumpToRoute = await showDialog<String>(
  //           context: context,
  //           barrierDismissible: false,
  //           builder: (_) => ScopeX(
  //             child: DocumentComposer(
  //               title: 'New subject',
  //               nameFieldLabel: 'Name',
  //               submitButton: SubmitButton(),
  //               sideBlocks: [
  //                 TypeSelectorBlock(),
  //                 ImageBlockWithCrop(cropCircle: true),
  //                 TagsBlock(),
  //               ],
  //             ),
  //           ),
  //         );

  //         if (jumpToRoute != null) {
  //           _pageContext.route = jumpToRoute;
  //           _pageContext.controller.jumpToPage(
  //             DateTime.now().millisecondsSinceEpoch,
  //           );
  //         }
  //       },
  //     ),
  //   );
  // }
}
