import 'package:flutter/material.dart';
import 'package:animated_text_kit/animated_text_kit.dart';
import 'package:auto_size_text/auto_size_text.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:rounded_loading_button/rounded_loading_button.dart';
import 'package:sliver_tools/sliver_tools.dart';

import '../../general/contexts/document_context.dart';
import '../widgets/clipped_avatar_container.dart';
import '../widgets/avatar_with_reputation_gauge.dart';
import '../../general/widgets/corner_banner.dart';
import '../../general/contexts/page_context.dart';
import '../bloc/subject_actions.dart';
import '../bloc/subject_bloc.dart';
import '../widgets/type_selector_block.dart';
import '../../general/widgets/document_composer.dart';
import '../../general/widgets/image_block_with_crop.dart';
import '../../general/widgets/tags_block.dart';
import '../../widget_extensions.dart';

class SubjectsPage extends StatefulWidget {
  const SubjectsPage({super.key});

  @override
  State<SubjectsPage> createState() => _SubjectsPageState();
}

class _SubjectsPageState extends StateX<SubjectsPage> {
  late final _pageContext = use<PageContext>();
  late final _subjectBloc = use<SubjectBloc>();

  @override
  void initState() {
    super.initState();
    _subjectBloc.dispatch(const GetSubjects());
  }

  @override
  Widget buildX(BuildContext context) {
    return StreamBuilder(
      stream: _subjectBloc.subjects$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return const SliverFillRemaining(
            hasScrollBody: false,
            child: Center(
              child: CircularProgressIndicator(),
            ),
          );
        }

        var subjects = snapshot.data!;
        if (subjects.isEmpty) {
          return const SliverFillRemaining(
            hasScrollBody: false,
            child: Center(
              child: Text('Nothing here yet'),
            ),
          );
        }

        return MultiSliver(
          children: [
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(25, 24, 25, 0),
                child: Row(
                  children: [
                    Container(
                      color: Colors.black,
                      width: 385,
                      padding: const EdgeInsets.all(8),
                      child: DefaultTextStyle(
                        style: GoogleFonts.righteous(
                          fontSize: 24,
                          color: Colors.white,
                        ),
                        child: Row(
                          children: [
                            const Text('> '),
                            AnimatedTextKit(
                              repeatForever: true,
                              pause: const Duration(seconds: 2),
                              animatedTexts: [
                                TypewriterAnimatedText(
                                  'Basically, a promise tracker',
                                  speed: const Duration(milliseconds: 70),
                                ),
                                TypewriterAnimatedText(
                                  'Blockchain never forgets',
                                  speed: const Duration(milliseconds: 70),
                                ),
                                TypewriterAnimatedText(
                                  'if (promiseKept) reputation++;',
                                  speed: const Duration(milliseconds: 70),
                                ),
                              ],
                            ),
                          ],
                        ),
                      ),
                    ),
                    const Spacer(),
                    ElevatedButton.icon(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: const Color(0xFF242423),
                        foregroundColor: const Color(0xffF8F9FA),
                        elevation: 10,
                      ),
                      icon: const Icon(Icons.search),
                      label: const Text('Search'),
                      onPressed: () {},
                    ),
                    const SizedBox(width: 12),
                    ElevatedButton.icon(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: const Color(0xffF8F9FA),
                        foregroundColor: const Color(0xFF242423),
                        elevation: 10,
                      ),
                      icon: const Icon(Icons.add),
                      label: const Text('Add'),
                      onPressed: () async {
                        var documentContext = DocumentContext();
                        var btnController = RoundedLoadingButtonController();

                        var subjectId = await showDialog<String>(
                          context: context,
                          barrierDismissible: false,
                          builder: (context) => ScopeX(
                            useInstances: [documentContext],
                            child: DocumentComposer(
                              title: 'New subject',
                              nameFieldLabel: 'Name',
                              submitButton: Padding(
                                padding: const EdgeInsets.symmetric(
                                  horizontal: 12,
                                ),
                                child: RoundedLoadingButton(
                                  child: const Text('Submit'),
                                  controller: btnController,
                                  onPressed: () async {
                                    var action = AddNewSubject(
                                      documentContext:
                                          DocumentContext.fromEditable(
                                        documentContext,
                                      ),
                                    );

                                    var success = await action.result;
                                    if (success == null) {
                                      btnController.error();
                                      await Future.delayed(
                                        const Duration(milliseconds: 1500),
                                      );
                                      btnController.reset();

                                      return;
                                    }

                                    btnController.success();
                                    await Future.delayed(
                                      const Duration(milliseconds: 1500),
                                    );
                                    if (context.mounted) {
                                      Navigator.of(context).pop(
                                        success.subjectId,
                                      );
                                    }
                                  },
                                ),
                              ),
                              sideBlocks: const [
                                TypeSelectorBlock(),
                                ImageBlockWithCrop(cropCircle: true),
                                TagsBlock(),
                              ],
                            ),
                          ),
                        );

                        if (subjectId != null) {
                          _pageContext.goto('/subjects/$subjectId');
                        }
                      },
                    ),
                  ],
                ),
              ),
            ),
            SliverPadding(
              padding: const EdgeInsets.fromLTRB(25, 20, 25, 24),
              sliver: SliverGrid(
                gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
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
                              ? const Color(0xffF8F9FA)
                              : const Color(0xFF242423),
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
                                                ? const Color(0xFF242423)
                                                : const Color(0xffF8F9FA),
                                            fontSize: 22,
                                          ),
                                          maxLines: 1,
                                          overflow: TextOverflow.ellipsis,
                                        ),
                                        const SizedBox(height: 12),
                                        RichText(
                                          text: TextSpan(
                                            children: [
                                              TextSpan(
                                                text: 'Settled promises: ',
                                                style: GoogleFonts.raleway(
                                                  color: blackOnWhite
                                                      ? const Color(0xFF242423)
                                                      : const Color(0xffF8F9FA),
                                                ),
                                              ),
                                              TextSpan(
                                                text: subject.settledThingsCount
                                                    .toString(),
                                                style: GoogleFonts.righteous(
                                                  color: blackOnWhite
                                                      ? const Color(0xFF242423)
                                                      : const Color(0xffF8F9FA),
                                                ),
                                              ),
                                            ],
                                          ),
                                        ),
                                        const SizedBox(height: 12),
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
                                                        ? const Color(
                                                            0xffF8F9FA)
                                                        : const Color(
                                                            0xFF242423),
                                                  ),
                                                  backgroundColor: blackOnWhite
                                                      ? const Color(0xFF242423)
                                                      : const Color(0xffF8F9FA),
                                                ),
                                              )
                                              .toList(),
                                        ),
                                      ],
                                    ),
                                  ),
                                  const SizedBox(width: 16),
                                  InkWell(
                                    onTap: () => _pageContext.goto(
                                      '/subjects/${subject.id}',
                                    ),
                                    borderRadius: const BorderRadius.only(
                                      topRight: Radius.circular(12),
                                      bottomRight: Radius.circular(12),
                                    ),
                                    child: Container(
                                      width: 42,
                                      height: double.infinity,
                                      decoration: BoxDecoration(
                                        color: blackOnWhite
                                            ? const Color(0xFF242423)
                                            : const Color(0xffF8F9FA),
                                        borderRadius: const BorderRadius.only(
                                          topRight: Radius.circular(12),
                                          bottomRight: Radius.circular(12),
                                        ),
                                      ),
                                      alignment: Alignment.center,
                                      child: Icon(
                                        Icons.arrow_forward_ios_rounded,
                                        color: blackOnWhite
                                            ? const Color(0xffF8F9FA)
                                            : const Color(0xFF242423),
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
                            value: subject.avgScore.toDouble(),
                            size: AvatarSize.small,
                            color: blackOnWhite
                                ? const Color(0xffF8F9FA)
                                : const Color(0xFF242423),
                          ),
                          color: blackOnWhite
                              ? const Color(0xFF242423)
                              : const Color(0xffF8F9FA),
                          fromNarrowToWide: (index ~/ 2) % 2 == 0,
                        ),
                        CornerBanner(
                          position: Alignment.topLeft,
                          size: 50,
                          cornerRadius: 12,
                          color: blackOnWhite
                              ? const Color(0xffF8F9FA)
                              : const Color(0xFF242423),
                          child: Icon(
                            subject.typeIcon,
                            size: 18,
                            color: blackOnWhite
                                ? const Color(0xFF242423)
                                : const Color(0xffF8F9FA),
                          ),
                        )
                      ],
                    );
                  },
                  childCount: subjects.length,
                ),
              ),
            ),
          ],
        );
      },
    );
  }
}
