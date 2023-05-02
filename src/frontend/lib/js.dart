@JS()
library app;

import 'dart:typed_data';

import 'package:js/js.dart';

external dynamic fetchAndResizeImage(String url);
external dynamic getImagePalette(String url);

@JS()
@anonymous
class ImageResult {
  external ByteBuffer get buffer;
  external String get mimeType;
}

@JS()
@anonymous
class PaletteResult {
  external List<ColorResult> colors;
}

@JS()
@anonymous
class ColorResult {
  external int get red;
  external int get green;
  external int get blue;
}

@JS('window.ethereum._state.initialized')
external bool isMetamaskInitialized;
