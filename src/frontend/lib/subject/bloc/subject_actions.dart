import '../../general/contexts/document_context.dart';
import '../../general/bloc/actions.dart';
import '../../general/bloc/mixins.dart';
import 'subject_result_vm.dart';

abstract class SubjectAction extends Action {
  const SubjectAction();
}

abstract class SubjectActionAwaitable<T extends SubjectResultVm?>
    extends SubjectAction with AwaitableResult<T> {}

class AddNewSubject extends SubjectAction {
  final DocumentContext documentContext;

  @override
  bool get mustValidate => true;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (documentContext.subjectType == null) {
      errors ??= [];
      errors.add('Type must be specified');
    }
    if (documentContext.nameOrTitle == null ||
        documentContext.nameOrTitle!.length < 3) {
      errors ??= [];
      errors.add('Name should be at least 3 characters long');
    }
    if (documentContext.details!.isEmpty) {
      // @@TODO: Figure out why it is never empty.
      errors ??= [];
      errors.add('Details are not specified');
    }

    return errors;
  }

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
