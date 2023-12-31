import '../../general/utils/utils.dart';
import '../../general/contexts/document_context.dart';
import '../../general/bloc/actions.dart';

abstract class SubjectAction extends Action {
  const SubjectAction();
}

class AddNewSubject extends SubjectAction {
  final DocumentContext documentContext;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (documentContext.subjectType == null) {
      errors ??= [];
      errors.add('Subject type must be specified');
    }
    if (documentContext.nameOrTitle == null || documentContext.nameOrTitle!.length < 3) {
      errors ??= [];
      errors.add('Name should be at least 3 characters long');
    }
    if (documentContext.operations!.isEmpty) {
      errors ??= [];
      errors.add('Details are not specified');
    }
    if (documentContext.imageExt == null ||
        documentContext.imageBytes == null ||
        documentContext.croppedImageBytes == null) {
      errors ??= [];
      errors.add('No image added');
    }
    if (documentContext.tags.isEmpty) {
      errors ??= [];
      errors.add('Tags are not specified');
    }

    return errors;
  }

  const AddNewSubject({required this.documentContext});
}

class GetSubjects extends SubjectAction {
  const GetSubjects();
}

class GetSubject extends SubjectAction {
  final String subjectId;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!subjectId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Subject Id');
    }

    return errors;
  }

  const GetSubject({required this.subjectId});
}

class GetThingsList extends SubjectAction {
  final String subjectId;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!subjectId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Subject Id');
    }

    return errors;
  }

  const GetThingsList({required this.subjectId});
}
