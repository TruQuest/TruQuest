enum DecisionIm {
  softDecline,
  hardDecline,
  accept,
}

extension DecisionImExtension on DecisionIm {
  String getString() {
    switch (this) {
      case DecisionIm.softDecline:
        return 'Soft decline';
      case DecisionIm.hardDecline:
        return 'Hard decline';
      case DecisionIm.accept:
        return 'Accept';
    }
  }
}
