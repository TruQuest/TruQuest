import 'dart:async';

import '../models/rvm/subject_preview_vm.dart';
import '../models/rvm/subject_vm.dart';
import '../models/rvm/thing_preview_vm.dart';
import 'subject_actions.dart';
import '../services/subject_service.dart';
import '../../general/bloc/bloc.dart';

class SubjectBloc extends Bloc<SubjectAction> {
  final SubjectService _subjectService;

  final _subjectsChannel = StreamController<List<SubjectPreviewVm>>.broadcast();
  Stream<List<SubjectPreviewVm>> get subjects$ => _subjectsChannel.stream;

  final _subjectChannel = StreamController<SubjectVm>.broadcast();
  Stream<SubjectVm> get subject$ => _subjectChannel.stream;

  final _thingsListChannel = StreamController<List<ThingPreviewVm>>.broadcast();
  Stream<List<ThingPreviewVm>> get thingsList$ => _thingsListChannel.stream;

  SubjectBloc(super.toastMessenger, this._subjectService) {
    actionChannel.stream.listen((action) {
      if (action is GetSubjects) {
        _getSubjects(action);
      } else if (action is GetSubject) {
        _getSubject(action);
      } else if (action is GetThingsList) {
        _getThingsList(action);
      }
    });
  }

  @override
  Future<Object?> handleExecute(SubjectAction action) {
    if (action is AddNewSubject) {
      return _addNewSubject(action);
    }

    throw UnimplementedError();
  }

  Future<String> _addNewSubject(AddNewSubject action) async {
    var subjectId = await _subjectService.addNewSubject(action.documentContext);
    return subjectId;
  }

  void _getSubjects(GetSubjects action) async {
    var subjects = await _subjectService.getSubjects();
    _subjectsChannel.add(subjects);
  }

  void _getSubject(GetSubject action) async {
    var subject = await _subjectService.getSubject(action.subjectId);
    _subjectChannel.add(subject);
  }

  void _getThingsList(GetThingsList action) async {
    var result = await _subjectService.getThingsList(action.subjectId);
    _thingsListChannel.add(result.things);
  }
}
