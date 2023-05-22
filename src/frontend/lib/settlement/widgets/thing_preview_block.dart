import 'package:flutter/material.dart';
import 'package:auto_size_text/auto_size_text.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../general/contexts/document_view_context.dart';
import '../../general/contexts/page_context.dart';
import '../models/rvm/settlement_proposal_vm.dart';
import '../../subject/widgets/clipped_image.dart';
import '../../widget_extensions.dart';

class ThingPreviewBlock extends StatelessWidgetX {
  late final _pageContext = use<PageContext>();
  late final _documentViewContext = useScoped<DocumentViewContext>();
  late final SettlementProposalVm _proposal = _documentViewContext.proposal!;

  ThingPreviewBlock({super.key});

  @override
  Widget buildX(BuildContext context) {
    return InkWell(
      borderRadius: BorderRadius.circular(12),
      onTap: () => _pageContext.goto('/things/${_proposal.thingId}'),
      child: Card(
        margin: const EdgeInsets.fromLTRB(10, 28, 10, 0),
        color: Color(0xffF8F9FA),
        elevation: 15,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
        ),
        child: Row(
          children: [
            ClippedImage(
              imageIpfsCid: _proposal.thingCroppedImageIpfsCid!,
              width: 140,
              height: 78.75,
              fromNarrowToWide: false,
            ),
            Expanded(
              child: SizedBox(
                height: 78.75,
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    AutoSizeText(
                      _proposal.subjectName,
                      style: GoogleFonts.philosopher(
                        color: Color(0xAA242423),
                        fontSize: 10,
                      ),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    SizedBox(height: 6),
                    AutoSizeText(
                      _proposal.thingTitle,
                      style: GoogleFonts.philosopher(
                        color: Color(0xFF242423),
                        fontSize: 14,
                      ),
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ],
                ),
              ),
            ),
            SizedBox(width: 8),
          ],
        ),
      ),
    );
  }
}
