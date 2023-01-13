import 'subject_result_vm.dart';
import 'subject_actions.dart';
import '../services/subject_service.dart';
import '../../general/bloc/bloc.dart';

class SubjectBloc extends Bloc<SubjectAction> {
  final SubjectService _subjectService;

  SubjectBloc(this._subjectService) {
    actionChannel.stream.listen((action) {
      if (action is AddNewSubject) {
        _addNewSubject(action);
      }
    });
  }

  @override
  void dispose({SubjectAction? cleanupAction}) {
    // TODO: implement dispose
  }

  void _addNewSubject(AddNewSubject action) async {
    var subjectId = await _subjectService.addNewSubject(action.documentContext);
    print('SubjectId: $subjectId');
    action.complete(AddNewSubjectSuccessResult(subjectId: subjectId));
  }
}
