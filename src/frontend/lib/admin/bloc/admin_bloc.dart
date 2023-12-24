import '../../general/bloc/bloc.dart';
import '../models/vm/command_output.dart';
import '../services/admin_service.dart';
import 'admin_actions.dart';

class AdminBloc extends Bloc<AdminAction> {
  final AdminService _adminService;

  Stream<List<CommandOutput>> get commandOutputs$ => _adminService.commandOutputs$;

  AdminBloc(super.toastMessenger, this._adminService);

  @override
  Future<Object?> handleExecute(AdminAction action) {
    if (action is CommandAction) {
      return _command(action);
    }

    throw UnimplementedError();
  }

  Future<bool> _command(CommandAction action) => _adminService.command(action.command, action.args, action.context);
}
