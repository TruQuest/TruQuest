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
}
