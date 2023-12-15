import 'package:flutter/material.dart';

enum VerdictVm {
  delivered,
  guessItCounts,
  aintGoodEnough,
  motionNotAction,
  noEffortWhatsoever,
  asGoodAsMaliciousIntent,
}

extension VerdictVmExtension on VerdictVm {
  String getString() {
    switch (this) {
      case VerdictVm.delivered:
        return 'Delivered';
      case VerdictVm.guessItCounts:
        return 'Guess it counts';
      case VerdictVm.aintGoodEnough:
        return 'Ain\'t good enough';
      case VerdictVm.motionNotAction:
        return 'Motion not action';
      case VerdictVm.noEffortWhatsoever:
        return 'No effort whatsoever';
      case VerdictVm.asGoodAsMaliciousIntent:
        return 'As good as malicious intent';
    }
  }

  Color getColor() {
    switch (this) {
      case VerdictVm.delivered:
        return const Color.fromARGB(255, 87, 26, 255);
      case VerdictVm.guessItCounts:
        return Colors.deepPurpleAccent;
      case VerdictVm.aintGoodEnough:
        return const Color.fromARGB(255, 240, 107, 255);
      case VerdictVm.motionNotAction:
        return const Color.fromARGB(255, 255, 107, 188);
      case VerdictVm.noEffortWhatsoever:
        return const Color(0xffFF6B6B);
      case VerdictVm.asGoodAsMaliciousIntent:
        return const Color.fromARGB(255, 174, 32, 58);
    }
  }
}
