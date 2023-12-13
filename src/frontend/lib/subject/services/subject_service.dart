import '../models/vm/get_things_list_rvm.dart';
import '../../general/contexts/document_context.dart';
import '../models/vm/subject_preview_vm.dart';
import '../models/vm/subject_vm.dart';
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
      documentContext.tags.toList(),
    );

    return subjectId;
  }

  Future<List<SubjectPreviewVm>> getSubjects() async {
    var subjects = await _subjectApiService.getSubjects();
    return subjects;
  }

  Future<SubjectVm> getSubject(String subjectId) async {
    var subject = await _subjectApiService.getSubject(subjectId);
    return subject;
  }

  Future<GetThingsListRvm> getThingsList(String subjectId) async {
    var result = await _subjectApiService.getThingsList(subjectId);
    return result;
  }
}
