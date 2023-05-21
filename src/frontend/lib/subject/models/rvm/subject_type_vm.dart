enum SubjectTypeVm {
  person,
  organization,
}

extension SubjectTypeVmExtension on SubjectTypeVm {
  String getString() =>
      this == SubjectTypeVm.person ? 'Person' : 'Organization';
}
