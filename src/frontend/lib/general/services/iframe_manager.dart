import 'dart:async';
import 'dart:html' as html;
import 'dart:ui' as ui;

import 'package:uuid/uuid.dart';

class IFrameManager {
  final IFrame iframePrivateKeyGen;
  final IFrame iframeQrCodeScan;

  IFrameManager()
      : iframePrivateKeyGen = IFrame(
          viewId: 'view-private-key-gen',
          url: 'http://localhost:5223/private-key-gen.html',
        ),
        iframeQrCodeScan = IFrame(
          viewId: 'view-qr-code-scan',
          url: 'http://localhost:5223/qr-code-scan.html',
          allowCamera: true,
        ) {
    iframePrivateKeyGen._init();
    iframeQrCodeScan._init();

    html.window.addEventListener('message', _handleMessage);
  }

  void _handleMessage(html.Event e) {
    if (e is html.MessageEvent && e.origin == 'http://localhost:5223') {
      print('***** MessageEvent from ${e.origin}: ${e.data} *****');
      var message = e.data as String;
      var messageSplit = message.split('|');
      var originName = messageSplit[0];
      var requestId = messageSplit[1];
      var content = messageSplit.skip(2).join('|');

      var recepient = originName == iframePrivateKeyGen.viewId ? iframePrivateKeyGen : iframeQrCodeScan;
      recepient._receiveMessage(requestId, content);
    }
  }
}

class IFrame {
  static final Uuid _uuid = Uuid();

  final String viewId;
  final String url;
  final bool allowCamera;

  late final html.IFrameElement _iframe;

  final Map<String, Completer<String>> _requestIdToResponseReceived = {};

  IFrame({
    required this.viewId,
    required this.url,
    this.allowCamera = false,
  });

  void _init() {
    _iframe = html.IFrameElement()..src = url;
    if (allowCamera) _iframe.allow = 'camera';
    _iframe.style
      ..border = 'none'
      ..height = '100%'
      ..width = '100%';

    // ignore: undefined_prefixed_name
    ui.platformViewRegistry.registerViewFactory(
      viewId,
      (int viewId) => _iframe,
    );
  }

  void _receiveMessage(String requestId, String message) =>
      _requestIdToResponseReceived.remove(requestId)?.complete(message);

  // @@NOTE: _iframe.contentWindow will be null unless we actually render the frame with
  // HtmlElementView.
  void _postMessage(String message) => _iframe.contentWindow!.postMessage(message, 'http://localhost:5223');

  Future<String> postMessageAndAwaitResponse(String message) {
    var requestId = _uuid.v4();
    var responseReceived = _requestIdToResponseReceived[requestId] = Completer<String>();
    _postMessage('$requestId|$message');

    return responseReceived.future;
  }
}
