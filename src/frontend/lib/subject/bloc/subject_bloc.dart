import 'dart:async';

import '../models/rvm/get_things_list_rvm.dart';
import '../models/rvm/subject_vm.dart';
import 'subject_result_vm.dart';
import 'subject_actions.dart';
import '../services/subject_service.dart';
import '../../general/bloc/bloc.dart';

class SubjectBloc extends Bloc<SubjectAction> {
  final SubjectService _subjectService;

  final StreamController<SubjectVm> _subjectChannel =
      StreamController<SubjectVm>.broadcast();
  Stream<SubjectVm> get subject$ => _subjectChannel.stream;

  final StreamController<GetThingsListRvm> _thingsListChannel =
      StreamController<GetThingsListRvm>.broadcast();
  Stream<GetThingsListRvm> get thingsList$ => _thingsListChannel.stream;

  SubjectBloc(this._subjectService) {
    actionChannel.stream.listen((action) {
      if (action is AddNewSubject) {
        _addNewSubject(action);
      } else if (action is GetSubject) {
        _getSubject(action);
      } else if (action is GetThingsList) {
        _getThingsList(action);
      }
    });
  }

  void _addNewSubject(AddNewSubject action) async {
    var subjectId = await _subjectService.addNewSubject(action.documentContext);
    print('SubjectId: $subjectId');
    action.complete(AddNewSubjectSuccessResult(subjectId: subjectId));
  }

  void _getSubject(GetSubject action) async {
    var subject = await _subjectService.getSubject(action.subjectId);
    _subjectChannel.add(subject);
  }

  void _getThingsList(GetThingsList action) async {
    var result = await _subjectService.getThingsList(action.subjectId);
    _thingsListChannel.add(result);
  }
}
