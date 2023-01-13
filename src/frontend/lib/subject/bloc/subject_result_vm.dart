abstract class SubjectResultVm {}

class AddNewSubjectSuccessResult extends SubjectResultVm {
  final String subjectId;

  AddNewSubjectSuccessResult({required this.subjectId});
}
