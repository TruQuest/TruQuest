import 'package:flutter/material.dart' hide Action;

import '../../general/bloc/actions.dart';

abstract class AdminAction extends Action {
  const AdminAction();
}

class CommandAction extends AdminAction {
  final String command;
  final List<String>? args;
  final BuildContext context;

  const CommandAction({
    required this.command,
    this.args,
    required this.context,
  });
}
