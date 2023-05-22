import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:stacked_card_carousel/stacked_card_carousel.dart';
import 'package:url_launcher/url_launcher.dart';

import '../contexts/document_view_context.dart';
import '../../thing/models/rvm/evidence_vm.dart';
import '../../widget_extensions.dart';

class EvidenceViewBlock extends StatefulWidget {
  const EvidenceViewBlock({super.key});

  @override
  State<EvidenceViewBlock> createState() => _EvidenceViewBlockState();
}

class _EvidenceViewBlockState extends StateX<EvidenceViewBlock> {
  late List<EvidenceVm> _evidence;

  final _pageController = PageController(initialPage: 0);

  @override
  void initState() {
    super.initState();
    Future.delayed(Duration(seconds: 1)).then(
      (_) => _pageController.animateToPage(
        _evidence.length - 1,
        duration: Duration(seconds: 3),
        curve: Curves.easeIn,
      ),
    );
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    _evidence = useScoped<DocumentViewContext>().evidence!;
  }

  @override
  void dispose() {
    _pageController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return StackedCardCarousel(
      pageController: _pageController,
      spaceBetweenItems: 220,
      items: _evidence
          .map(
            (e) => Card(
              margin: const EdgeInsets.symmetric(horizontal: 12),
              elevation: 10,
              clipBehavior: Clip.antiAlias,
              shadowColor: Colors.white,
              child: Column(
                children: [
                  Image.network(
                    'http://localhost:8080/ipfs/' + e.previewImageIpfsCid,
                    width: double.infinity,
                  ),
                  SizedBox(height: 8),
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 12),
                    child: Text(
                      e.originUrl,
                      overflow: TextOverflow.fade,
                      softWrap: false,
                      style: GoogleFonts.raleway(),
                    ),
                  ),
                  SizedBox(height: 8),
                  ElevatedButton.icon(
                    onPressed: () async {
                      await launchUrl(
                        Uri.parse('http://localhost:8080/ipfs/' + e.ipfsCid),
                      );
                    },
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Color.fromARGB(255, 183, 166, 190),
                      foregroundColor: Colors.white,
                    ),
                    icon: Icon(
                      Icons.launch,
                      size: 18,
                    ),
                    label: Text('Open'),
                  ),
                  SizedBox(height: 8),
                ],
              ),
            ),
          )
          .toList(),
    );
  }
}
