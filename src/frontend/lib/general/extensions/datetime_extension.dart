import 'package:intl/intl.dart';

extension DateTimeExtension on DateTime {
  String getString() {
    var s = DateFormat('yyyy-MM-dd HH:mm:ss').format(this);
    Duration offset = timeZoneOffset;
    int hours = offset.inHours > 0 ? offset.inHours : 1;

    if (!offset.isNegative) {
      s += '+' +
          offset.inHours.toString().padLeft(2, '0') +
          ':' +
          (offset.inMinutes % (hours * 60)).toString().padLeft(2, '0');
    } else {
      s += '-' +
          (-offset.inHours).toString().padLeft(2, '0') +
          ':' +
          (offset.inMinutes % (hours * 60)).toString().padLeft(2, '0');
    }

    return s;
  }
}
