@JS()
library app;

import 'dart:typed_data';

import 'package:js/js.dart';

external dynamic fetchAndResizeImage(String url);

@JS()
@anonymous
class ImageResult {
  external ByteBuffer get buffer;
  external String get mimeType;
}
