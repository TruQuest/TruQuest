import 'actions.dart';

abstract class GeneralAction extends Action {
  const GeneralAction();
}

class GetTags extends GeneralAction {
  const GetTags();
}

class GetContractsStates extends GeneralAction {
  const GetContractsStates();
}
