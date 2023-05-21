import 'dart:math';

import 'package:flutter/material.dart';
import 'package:animated_text_kit/animated_text_kit.dart';
import 'package:flutter_card_swiper/flutter_card_swiper.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../general/contexts/page_context.dart';
import '../../thing/models/rvm/thing_state_vm.dart';
import '../../general/contexts/document_view_context.dart';
import '../../widget_extensions.dart';
import '../models/rvm/thing_preview_vm.dart';
import '../../settlement/models/rvm/verdict_vm.dart';

class LatestThingsBlock extends StatelessWidgetX {
  late final _pageContext = use<PageContext>();
  late final _documentViewContext = useScoped<DocumentViewContext>();

  late final List<ThingPreviewVm> _latestSettledThings =
      _documentViewContext.subject!.latestSettledThings;
  late final List<ThingPreviewVm> _latestUnsettledThings =
      _documentViewContext.subject!.latestUnsettledThings;

  Widget _buildThingPreviewCard(ThingPreviewVm? thing, String placeholderText) {
    return InkWell(
      borderRadius: BorderRadius.circular(10),
      onTap:
          thing != null ? () => _pageContext.goto('/things/${thing.id}') : null,
      child: Container(
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(10),
          color: Colors.white,
          boxShadow: [
            BoxShadow(
              color: Colors.grey.withOpacity(0.2),
              spreadRadius: 3,
              blurRadius: 7,
              offset: Offset(0, 3),
            ),
          ],
        ),
        clipBehavior: Clip.hardEdge,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            AspectRatio(
              aspectRatio: 0.95,
              child: thing != null
                  ? Image.network(
                      'http://localhost:8080/ipfs/' +
                          thing.croppedImageIpfsCid!,
                      fit: BoxFit.cover,
                    )
                  : Container(
                      decoration: BoxDecoration(
                        gradient: LinearGradient(
                          begin: Alignment.topCenter,
                          end: Alignment.bottomCenter,
                          colors: [
                            Color(0xFFFF3868),
                            Color(0xFFFFB49A),
                          ],
                        ),
                      ),
                    ),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(10, 8, 10, 0),
              child: Text(
                thing?.title ?? placeholderText,
                style: GoogleFonts.philosopher(
                  color: Colors.black,
                  fontSize: 20,
                ),
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
              ),
            ),
            if (thing != null)
              Padding(
                padding: const EdgeInsets.fromLTRB(10, 10, 10, 0),
                child: Text(
                  thing.verdict?.getString() ?? thing.state.getString(),
                  style: GoogleFonts.raleway(
                    color: Colors.grey,
                    fontSize: 16,
                  ),
                ),
              ),
            SizedBox(height: 8),
          ],
        ),
      ),
    );
  }

  @override
  Widget buildX(BuildContext context) {
    return Column(
      children: [
        Container(
          margin: const EdgeInsets.fromLTRB(42, 24, 8, 0),
          color: Colors.black,
          width: double.infinity,
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
                      'Latest',
                      speed: Duration(milliseconds: 70),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
        SizedBox(
          // @@!!: This height value must be dynamically computed.
          // 460 is simply hardcoded for my screen.
          height: 460,
          child: Row(
            children: [
              RotatedBox(
                quarterTurns: 3,
                child: Padding(
                  padding: const EdgeInsets.only(top: 10),
                  child: Text(
                    'Unsettled',
                    style: GoogleFonts.righteous(
                      color: Colors.white,
                      fontSize: 22,
                    ),
                  ),
                ),
              ),
              Expanded(
                child: CardSwiper(
                  isDisabled: _latestUnsettledThings.isEmpty,
                  cardBuilder: (context, index) {
                    var thing = _latestUnsettledThings.isNotEmpty
                        ? _latestUnsettledThings[index]
                        : null;

                    return _buildThingPreviewCard(
                      thing,
                      'No unsettled promises yet',
                    );
                  },
                  cardsCount: max(_latestUnsettledThings.length, 1),
                  numberOfCardsDisplayed: max(_latestUnsettledThings.length, 1),
                  isVerticalSwipingEnabled: false,
                ),
              ),
            ],
          ),
        ),
        SizedBox(
          height: 460,
          child: Row(
            children: [
              RotatedBox(
                quarterTurns: 3,
                child: Padding(
                  padding: const EdgeInsets.only(top: 10),
                  child: Text(
                    'Settled',
                    style: GoogleFonts.righteous(
                      color: Colors.white,
                      fontSize: 22,
                    ),
                  ),
                ),
              ),
              Expanded(
                child: CardSwiper(
                  isDisabled: _latestSettledThings.isEmpty,
                  cardBuilder: (context, index) {
                    var thing = _latestSettledThings.isNotEmpty
                        ? _latestSettledThings[index]
                        : null;

                    return _buildThingPreviewCard(
                      thing,
                      'No settled promises yet',
                    );
                  },
                  cardsCount: max(_latestSettledThings.length, 1),
                  numberOfCardsDisplayed: max(_latestSettledThings.length, 1),
                  isVerticalSwipingEnabled: false,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
