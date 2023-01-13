import '../../general/contexts/document_context.dart';
import 'subject_api_service.dart';

class SubjectService {
  final SubjectApiService _subjectApiService;

  SubjectService(this._subjectApiService);

  Future<String> addNewSubject(DocumentContext documentContext) async {
    var subjectId = await _subjectApiService.addNewSubject(
      documentContext.subjectType!,
      documentContext.nameOrTitle!,
      documentContext.details!,
      documentContext.imageExt!,
      documentContext.imageBytes!,
      documentContext.croppedImageBytes!,
      [1],
    );

    return subjectId;
  }
}
