enum DecisionVm {
  softDecline,
  hardDecline,
  accept,
}

extension DecisionVmExtension on DecisionVm {
  String getString() {
    switch (this) {
      case DecisionVm.softDecline:
        return 'Soft decline';
      case DecisionVm.hardDecline:
        return 'Hard decline';
      case DecisionVm.accept:
        return 'Accept';
    }
  }
}
