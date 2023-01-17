import 'package:convert/convert.dart';
import 'package:uuid/uuid.dart';

extension UuidExtension on String {
  String toSolInputFormat() {
    var thingIdBytes = Uuid.parse(this, validate: false);
    int b0 = thingIdBytes[0];
    int b1 = thingIdBytes[1];
    int b2 = thingIdBytes[2];
    int b3 = thingIdBytes[3];
    thingIdBytes[0] = b3;
    thingIdBytes[1] = b2;
    thingIdBytes[2] = b1;
    thingIdBytes[3] = b0;

    int b4 = thingIdBytes[4];
    int b5 = thingIdBytes[5];
    thingIdBytes[4] = b5;
    thingIdBytes[5] = b4;

    int b6 = thingIdBytes[6];
    int b7 = thingIdBytes[7];
    thingIdBytes[6] = b7;
    thingIdBytes[7] = b6;

    return '0x' + hex.encode(thingIdBytes);
  }
}
