enum VerdictIm {
  delivered,
  guessItCounts,
  aintGoodEnough,
  motionNotAction,
  noEffortWhatsoever,
  asGoodAsMaliciousIntent,
}

extension VerdictImExtension on VerdictIm {
  String getString() {
    switch (this) {
      case VerdictIm.delivered:
        return 'Delivered';
      case VerdictIm.guessItCounts:
        return 'Guess it counts';
      case VerdictIm.aintGoodEnough:
        return 'Ain\'t good enough';
      case VerdictIm.motionNotAction:
        return 'Motion not action';
      case VerdictIm.noEffortWhatsoever:
        return 'No effort whatsoever';
      case VerdictIm.asGoodAsMaliciousIntent:
        return 'As good as malicious intent';
    }
  }
}
