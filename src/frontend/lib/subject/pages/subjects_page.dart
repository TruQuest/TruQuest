import 'package:flutter/material.dart';
import 'package:animated_text_kit/animated_text_kit.dart';
import 'package:auto_size_text/auto_size_text.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:sliver_tools/sliver_tools.dart';

import '../widgets/clipped_avatar_container.dart';
import '../widgets/avatar_with_reputation_gauge.dart';
import '../../general/widgets/corner_banner.dart';
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
  late final _pageContext = use<PageContext>();
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
          return SliverFillRemaining(
            hasScrollBody: false,
            child: Center(
              child: CircularProgressIndicator(),
            ),
          );
        }

        var subjects = snapshot.data!;
        if (subjects.isEmpty) {
          return SliverFillRemaining(
            hasScrollBody: false,
            child: Center(
              child: Text('Nothing here yet'),
            ),
          );
        }

        subjects = Iterable.generate(10, (_) => subjects.first).toList();

        return MultiSliver(
          children: [
            SliverCrossAxisConstrained(
              maxCrossAxisExtent: 1450,
              alignment: 0,
              child: SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.only(top: 24),
                  child: Row(
                    children: [
                      Container(
                        color: Colors.black,
                        width: 370,
                        padding: const EdgeInsets.all(8),
                        child: DefaultTextStyle(
                          style: GoogleFonts.righteous(
                            fontSize: 24,
                            color: Colors.white,
                          ),
                          child: Row(
                            children: [
                              Text('> '),
                              AnimatedTextKit(
                                repeatForever: true,
                                pause: Duration(seconds: 2),
                                animatedTexts: [
                                  TypewriterAnimatedText(
                                    'Basically, a promise tracker',
                                    speed: Duration(milliseconds: 70),
                                  ),
                                  TypewriterAnimatedText(
                                    'Blockchain never forgets',
                                    speed: Duration(milliseconds: 70),
                                  ),
                                ],
                              ),
                            ],
                          ),
                        ),
                      ),
                      Spacer(),
                      ElevatedButton.icon(
                        style: ElevatedButton.styleFrom(
                          backgroundColor: Color(0xFF242423),
                          foregroundColor: Color(0xffF8F9FA),
                          elevation: 10,
                        ),
                        icon: Icon(Icons.search),
                        label: Text('Search'),
                        onPressed: () {},
                      ),
                      SizedBox(width: 12),
                      ElevatedButton.icon(
                        style: ElevatedButton.styleFrom(
                          backgroundColor: Color(0xffF8F9FA),
                          foregroundColor: Color(0xFF242423),
                          elevation: 10,
                        ),
                        icon: Icon(Icons.add),
                        label: Text('Add'),
                        onPressed: () {},
                      ),
                    ],
                  ),
                ),
              ),
            ),
            SliverPadding(
              padding: const EdgeInsets.only(top: 20, bottom: 24),
              sliver: SliverCrossAxisConstrained(
                maxCrossAxisExtent: 1450,
                alignment: 0,
                child: SliverGrid(
                  gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                    crossAxisCount: 2,
                    mainAxisSpacing: 22,
                    crossAxisSpacing: 50,
                    mainAxisExtent: 200,
                  ),
                  delegate: SliverChildBuilderDelegate(
                    (context, index) {
                      var subject = subjects[index];
                      var blackOnWhite = index % 2 == 0;

                      return Stack(
                        children: [
                          Card(
                            margin: EdgeInsets.zero,
                            color: blackOnWhite
                                ? Color(0xffF8F9FA)
                                : Color(0xFF242423),
                            elevation: 15,
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(12),
                            ),
                            child: Padding(
                              padding: const EdgeInsets.only(left: 250),
                              child: SizedBox(
                                width: 450,
                                height: double.infinity,
                                child: Row(
                                  children: [
                                    Expanded(
                                      child: Column(
                                        mainAxisAlignment:
                                            MainAxisAlignment.center,
                                        crossAxisAlignment:
                                            CrossAxisAlignment.start,
                                        children: [
                                          AutoSizeText(
                                            subject.name,
                                            style: GoogleFonts.philosopher(
                                              color: blackOnWhite
                                                  ? Color(0xFF242423)
                                                  : Color(0xffF8F9FA),
                                              fontSize: 22,
                                            ),
                                            maxLines: 1,
                                            overflow: TextOverflow.ellipsis,
                                          ),
                                          SizedBox(height: 12),
                                          RichText(
                                            text: TextSpan(
                                              children: [
                                                TextSpan(
                                                  text: 'Settled promises: ',
                                                  style: GoogleFonts.raleway(
                                                    color: blackOnWhite
                                                        ? Color(0xFF242423)
                                                        : Color(0xffF8F9FA),
                                                  ),
                                                ),
                                                TextSpan(
                                                  text:
                                                      (subject.settledThingsCount ??
                                                              0)
                                                          .toString(),
                                                  style: GoogleFonts.righteous(
                                                    color: blackOnWhite
                                                        ? Color(0xFF242423)
                                                        : Color(0xffF8F9FA),
                                                  ),
                                                ),
                                              ],
                                            ),
                                          ),
                                          SizedBox(height: 12),
                                          Wrap(
                                            spacing: 8,
                                            runSpacing: 6,
                                            children: subject.tags
                                                .map(
                                                  (tag) => Chip(
                                                    label: Text(tag.name),
                                                    labelStyle:
                                                        GoogleFonts.righteous(
                                                      color: blackOnWhite
                                                          ? Color(0xffF8F9FA)
                                                          : Color(0xFF242423),
                                                    ),
                                                    backgroundColor:
                                                        blackOnWhite
                                                            ? Color(0xFF242423)
                                                            : Color(0xffF8F9FA),
                                                  ),
                                                )
                                                .toList(),
                                          ),
                                        ],
                                      ),
                                    ),
                                    SizedBox(width: 16),
                                    InkWell(
                                      onTap: () => _pageContext.goto(
                                        '/subjects/${subject.id}',
                                      ),
                                      borderRadius: BorderRadius.only(
                                        topRight: Radius.circular(12),
                                        bottomRight: Radius.circular(12),
                                      ),
                                      child: Container(
                                        width: 42,
                                        height: double.infinity,
                                        decoration: BoxDecoration(
                                          color: blackOnWhite
                                              ? Color(0xFF242423)
                                              : Color(0xffF8F9FA),
                                          borderRadius: BorderRadius.only(
                                            topRight: Radius.circular(12),
                                            bottomRight: Radius.circular(12),
                                          ),
                                        ),
                                        alignment: Alignment.center,
                                        child: Icon(
                                          Icons.arrow_forward_ios_rounded,
                                          color: blackOnWhite
                                              ? Color(0xffF8F9FA)
                                              : Color(0xFF242423),
                                        ),
                                      ),
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
                              color: blackOnWhite
                                  ? Color(0xffF8F9FA)
                                  : Color(0xFF242423),
                            ),
                            color: blackOnWhite
                                ? Color(0xFF242423)
                                : Color(0xffF8F9FA),
                            fromNarrowToWide: (index ~/ 2) % 2 == 0,
                          ),
                          CornerBanner(
                            position: Alignment.topLeft,
                            size: 50,
                            cornerRadius: 12,
                            color: blackOnWhite
                                ? Color(0xffF8F9FA)
                                : Color(0xFF242423),
                            child: Icon(
                              subject.typeIcon,
                              size: 18,
                              color: blackOnWhite
                                  ? Color(0xFF242423)
                                  : Color(0xffF8F9FA),
                            ),
                          )
                        ],
                      );
                    },
                    childCount: subjects.length,
                  ),
                ),
              ),
            ),
          ],
        );
      },
    );
  }
}
