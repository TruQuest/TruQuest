import 'package:flutter/material.dart' hide Action;

import '../../general/bloc/actions.dart';

abstract class AdminAction extends Action {
  const AdminAction();
}

class CommandAction extends AdminAction {
  static final _validCommands = [
    'help',
    'exit',
    'logs',
    'admin',
    'goto',
    'set',
    'access',
    'whitelist',
  ];

  final String command;
  final List<String>? args;
  final BuildContext context;

  @override
  List<String>? validate() {
    if (!_validCommands.contains(command)) {
      return ['Invalid command'];
    }

    switch (command) {
      case 'help':
      case 'exit':
      case 'logs':
      case 'admin':
        if (args != null) return ['This command doesn\'t take any arguments'];
      case 'goto':
        if (args == null || args!.length != 1) return ['This command takes exactly 1 argument'];
      case 'set':
        if (args == null || args!.length != 2) return ['This command takes exactly 2 arguments'];
        if (args!.first != 's_withdrawalsEnabled' && args!.first != 's_stopTheWorld')
          return ["First argument can be either 's_withdrawalsEnabled' or 's_stopTheWorld'"];
        if (args!.last != 'true' && args!.last != 'false') return ["Second argument can be either 'true' or 'false'"];
      case 'access':
        if (args == null || args!.length != 2) return ['This command takes exactly 2 arguments'];
        if (args!.first != 'give' && args!.first != 'remove')
          return ["First argument can be either 'give' or 'remove'"];
      case 'whitelist':
        if (args == null || args!.length != 2) return ['This command takes exactly 2 arguments'];
        if (args!.first != 'add' && args!.first != 'remove') return ["First argument can be either 'add' or 'remove'"];
    }

    return null;
  }

  const CommandAction({
    required this.command,
    this.args,
    required this.context,
  });
}
