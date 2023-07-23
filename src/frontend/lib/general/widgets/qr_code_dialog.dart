import 'package:flutter/material.dart';
import 'package:qr_flutter/qr_flutter.dart';

class QrCodeDialog extends StatelessWidget {
  final String uri;

  const QrCodeDialog({super.key, required this.uri});

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Scan the QR code'),
      content: Container(
        width: 500,
        height: 500,
        alignment: Alignment.center,
        child: QrImageView(
          data: uri,
          version: QrVersions.auto,
          size: 400,
        ),
      ),
    );
  }
}
