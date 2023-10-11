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
        return Color.fromARGB(255, 72, 206, 119);
      case VerdictVm.guessItCounts:
        return Color.fromARGB(255, 122, 226, 129);
      case VerdictVm.aintGoodEnough:
        return Color.fromARGB(255, 210, 255, 60);
      case VerdictVm.motionNotAction:
        return Color.fromARGB(255, 255, 202, 141);
      case VerdictVm.noEffortWhatsoever:
        return Color(0xffFF6B6B);
      case VerdictVm.asGoodAsMaliciousIntent:
        return Color.fromARGB(255, 203, 62, 88);
    }
  }
}
