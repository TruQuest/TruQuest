import '../../general/contexts/document_context.dart';
import '../../general/bloc/mixins.dart';
import 'subject_result_vm.dart';

abstract class SubjectAction {}

abstract class SubjectActionAwaitable<T extends SubjectResultVm?>
    extends SubjectAction with AwaitableResult<T> {}

class AddNewSubject
    extends SubjectActionAwaitable<AddNewSubjectSuccessResult?> {
  final DocumentContext documentContext;

  AddNewSubject({required this.documentContext});
}

class GetSubject extends SubjectAction {
  final String subjectId;

  GetSubject({required this.subjectId});
}
