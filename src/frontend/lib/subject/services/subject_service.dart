import '../models/rvm/get_things_list_rvm.dart';
import '../../general/contexts/document_context.dart';
import '../models/rvm/subject_preview_vm.dart';
import '../models/rvm/subject_vm.dart';
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
