import '../../general/contexts/document_context.dart';
import '../../general/bloc/mixins.dart';
import 'subject_result_vm.dart';

abstract class SubjectAction {
  const SubjectAction();
}

abstract class SubjectActionAwaitable<T extends SubjectResultVm?>
    extends SubjectAction with AwaitableResult<T> {}

class AddNewSubject
    extends SubjectActionAwaitable<AddNewSubjectSuccessResult?> {
  final DocumentContext documentContext;

  AddNewSubject({required this.documentContext});
}

class GetSubjects extends SubjectAction {
  const GetSubjects();
}

class GetSubject extends SubjectAction {
  final String subjectId;

  const GetSubject({required this.subjectId});
}

class GetThingsList extends SubjectAction {
  final String subjectId;

  const GetThingsList({required this.subjectId});
}
