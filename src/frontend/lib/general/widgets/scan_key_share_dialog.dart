import 'dart:html' as html;

import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../widget_extensions.dart';
import '../services/iframe_manager.dart';

class ScanKeyShareDialog extends StatefulWidget {
  final String scanRequestId;

  ScanKeyShareDialog({super.key, required this.scanRequestId});

  @override
  State<ScanKeyShareDialog> createState() => _ScanKeyShareDialogState();
}

class _ScanKeyShareDialogState extends StateX<ScanKeyShareDialog> {
  late final _iframeManager = use<IFrameManager>();

  @override
  void initState() {
    super.initState();
    html.window.addEventListener('message', _handleMessage);
  }

  @override
  void dispose() {
    html.window.removeEventListener('message', _handleMessage);
    super.dispose();
  }

  void _handleMessage(html.Event e) {
    if (e is html.MessageEvent && e.origin == dotenv.env['ORCHESTRATOR_HOST']) {
      if (e.data is! String) return;

      var message = e.data as String;
      var messageSplit = message.split('|');
      var originViewId = messageSplit[0];
      var requestId = messageSplit[1];
      // @@NOTE: Zero uuid means not a /response/ but a simple message from iframe.
      if (originViewId == _iframeManager.iframeQrCodeScan.viewId &&
          requestId == '00000000-0000-0000-0000-000000000000' &&
          messageSplit[2] == widget.scanRequestId) {
        if (context.mounted) Navigator.of(context).pop();
      }
    }
  }

  @override
  Widget buildX(BuildContext context) {
    return SimpleDialog(
      backgroundColor: const Color(0xFF242423),
      title: Text(
        'Scan or upload',
        style: GoogleFonts.philosopher(
          color: Colors.white,
          fontSize: 24,
        ),
      ),
      children: [
        Container(
          width: 650,
          height: 650,
          alignment: Alignment.center,
          child: HtmlElementView(viewType: _iframeManager.iframeQrCodeScan.viewId),
        ),
      ],
    );
  }
}
