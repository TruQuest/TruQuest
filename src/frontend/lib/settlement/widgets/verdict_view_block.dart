import 'package:flutter/material.dart';
import 'package:animated_text_kit/animated_text_kit.dart';
import 'package:google_fonts/google_fonts.dart';

import '../models/rvm/verdict_vm.dart';
import '../../general/contexts/document_view_context.dart';
import '../../widget_extensions.dart';

// ignore: must_be_immutable
class VerdictViewBlock extends StatelessWidgetX {
  late final _documentViewContext = useScoped<DocumentViewContext>();
  late final VerdictVm _verdict = _documentViewContext.proposal!.verdict;

  VerdictViewBlock({super.key});

  @override
  Widget buildX(BuildContext context) {
    return DefaultTextStyle(
      style: GoogleFonts.righteous(
        fontSize: 24,
        color: Colors.white,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            margin: const EdgeInsets.fromLTRB(16, 24, 16, 0),
            color: Colors.black,
            width: 150,
            padding: const EdgeInsets.all(8),
            child: Text(
              '> Verdict:',
              style: TextStyle(fontSize: 20),
            ),
          ),
          Container(
            margin: const EdgeInsets.fromLTRB(16, 8, 16, 16),
            color: Colors.black,
            width: double.infinity,
            height: 50,
            padding: const EdgeInsets.all(8),
            child: Row(
              children: [
                Text('> '),
                Expanded(
                  child: FittedBox(
                    alignment: Alignment.centerLeft,
                    child: AnimatedTextKit(
                      repeatForever: true,
                      pause: Duration(seconds: 2),
                      animatedTexts: [
                        TypewriterAnimatedText(
                          _verdict.getString(),
                          speed: Duration(milliseconds: 70),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
