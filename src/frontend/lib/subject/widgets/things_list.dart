import 'package:flutter/material.dart';
import 'package:animated_text_kit/animated_text_kit.dart';
import 'package:auto_size_text/auto_size_text.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:rounded_loading_button/rounded_loading_button.dart';

import '../../general/widgets/restrict_when_unauthorized_button.dart';
import '../../thing/bloc/thing_actions.dart';
import '../../thing/bloc/thing_bloc.dart';
import '../../general/contexts/page_context.dart';
import '../../general/contexts/document_context.dart';
import '../../general/widgets/document_composer.dart';
import '../../general/widgets/evidence_block.dart';
import '../../general/widgets/image_block_with_crop.dart';
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
  late final _thingBloc = use<ThingBloc>();

  @override
  void initState() {
    super.initState();
    _subjectBloc.dispatch(GetThingsList(subjectId: widget.subjectId));
  }

  @override
  Widget buildX(BuildContext context) {
    return StreamBuilder(
      stream: _subjectBloc.thingsList$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return const Center(child: CircularProgressIndicator());
        }

        var things = snapshot.data!.things;

        return Center(
          child: Column(
            children: [
              Padding(
                padding: const EdgeInsets.fromLTRB(25, 40, 25, 0),
                child: Row(
                  children: [
                    Container(
                      color: Colors.black,
                      width: 350,
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
                                  'Blockchain never forgets',
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
                    RestrictWhenUnauthorizedButton(
                      child: ElevatedButton.icon(
                        style: ElevatedButton.styleFrom(
                          backgroundColor: const Color(0xffF8F9FA),
                          foregroundColor: const Color(0xFF242423),
                          elevation: 10,
                        ),
                        icon: const Icon(Icons.add),
                        label: const Text('Add'),
                        onPressed: () {
                          var documentContext = DocumentContext();
                          documentContext.subjectId = widget.subjectId;

                          var btnController = RoundedLoadingButtonController();

                          showDialog(
                            context: context,
                            barrierDismissible: false,
                            builder: (context) => ScopeX(
                              useInstances: [documentContext],
                              child: DocumentComposer(
                                title: 'New promise',
                                nameFieldLabel: 'Title',
                                submitButton: Padding(
                                  padding: const EdgeInsets.symmetric(
                                    horizontal: 12,
                                  ),
                                  child: RoundedLoadingButton(
                                    child: const Text('Prepare draft'),
                                    controller: btnController,
                                    onPressed: () async {
                                      var action = CreateNewThingDraft(
                                        documentContext:
                                            DocumentContext.fromEditable(
                                                documentContext),
                                      );
                                      _thingBloc.dispatch(action);

                                      var failure = await action.result;
                                      if (failure != null) {
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
                                        Navigator.of(context).pop();
                                      }
                                    },
                                  ),
                                ),
                                sideBlocks: const [
                                  ImageBlockWithCrop(cropCircle: false),
                                  TagsBlock(),
                                  EvidenceBlock(),
                                ],
                              ),
                            ),
                          );
                        },
                      ),
                    ),
                  ],
                ),
              ),
              Expanded(
                child: Padding(
                  padding: const EdgeInsets.fromLTRB(25, 20, 25, 24),
                  child: GridView.builder(
                    gridDelegate:
                        const SliverGridDelegateWithFixedCrossAxisCount(
                      crossAxisCount: 2,
                      mainAxisSpacing: 22,
                      crossAxisSpacing: 50,
                      mainAxisExtent: 135,
                    ),
                    itemBuilder: (context, index) {
                      var thing = things[index];
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
                                            thing.title,
                                            style: GoogleFonts.philosopher(
                                              color: blackOnWhite
                                                  ? const Color(0xFF242423)
                                                  : const Color(0xffF8F9FA),
                                              fontSize: 18,
                                            ),
                                            maxLines: 2,
                                            overflow: TextOverflow.ellipsis,
                                          ),
                                          const SizedBox(height: 12),
                                          Text(
                                            thing.displayedTimestampFormatted,
                                            style: GoogleFonts.raleway(
                                              color: blackOnWhite
                                                  ? const Color(0xFF242423)
                                                  : const Color(0xffF8F9FA),
                                            ),
                                          ),
                                          const SizedBox(height: 12),
                                          Wrap(
                                            spacing: 8,
                                            runSpacing: 6,
                                            children: thing.tags
                                                .map(
                                                  (tag) => Chip(
                                                    label: Text(tag.name),
                                                    labelStyle:
                                                        GoogleFonts.righteous(
                                                      color: blackOnWhite
                                                          ? const Color(
                                                              0xffF8F9FA,
                                                            )
                                                          : const Color(
                                                              0xFF242423,
                                                            ),
                                                    ),
                                                    backgroundColor:
                                                        blackOnWhite
                                                            ? const Color(
                                                                0xFF242423,
                                                              )
                                                            : const Color(
                                                                0xffF8F9FA,
                                                              ),
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
                                        '/things/${thing.id}',
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
                          ClippedImage(
                            imageIpfsCid: thing.croppedImageIpfsCid!,
                            width: 240,
                            height: 135,
                            fromNarrowToWide: (index ~/ 2) % 2 == 1,
                          ),
                          CornerBanner(
                            position: Alignment.topLeft,
                            size: 50,
                            cornerRadius: 12,
                            color: thing.verdict != null
                                ? thing.verdictColor
                                : blackOnWhite
                                    ? const Color(0xffF8F9FA)
                                    : const Color(0xFF242423),
                            child: Icon(
                              thing.stateIcon,
                              size: 18,
                              color: thing.verdict != null
                                  ? Colors.white
                                  : blackOnWhite
                                      ? const Color(0xFF242423)
                                      : const Color(0xffF8F9FA),
                            ),
                          ),
                        ],
                      );
                    },
                    itemCount: things.length,
                  ),
                ),
              ),
            ],
          ),
        );
      },
    );
  }
}
