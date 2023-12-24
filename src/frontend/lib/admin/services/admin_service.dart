import 'package:flutter/material.dart';
import 'package:rxdart/rxdart.dart';
import 'package:talker_flutter/talker_flutter.dart';

import '../../general/contexts/page_context.dart';
import '../../general/utils/logger.dart';
import '../models/vm/command_output.dart';
import '../widgets/admin_panel_dialog.dart';
import 'admin_api_service.dart';

class AdminService {
  final AdminApiService _adminApiService;
  final PageContext _pageContext;

  final List<CommandOutput> _commandOutputs = [];

  final _commandOutputsChannel = BehaviorSubject<List<CommandOutput>>();
  Stream<List<CommandOutput>> get commandOutputs$ => _commandOutputsChannel.stream;

  AdminService(this._adminApiService, this._pageContext);

  void _addCommandOutput(String command, {List<String>? args, List<String>? outputLines}) {
    _commandOutputs.insert(
      0,
      CommandOutput(
        commandLine: '> $command ${args?.join(' ') ?? ''}'.trimRight(),
        outputLines: outputLines,
      ),
    );
    _commandOutputsChannel.add(_commandOutputs);
  }

  Future<bool> command(String command, List<String>? args, BuildContext context) async {
    switch (command) {
      case 'help':
        _addCommandOutput('help', outputLines: [
          'help',
          'exit',
          'logs',
          'admin',
          'goto',
          'set',
          'access',
          'whitelist',
        ]);

        return true;
      case 'exit':
        return false;
      case 'logs':
        showDialog(
          context: context,
          builder: (_) => SimpleDialog(
            children: [
              SizedBox(
                width: 1000,
                height: 600,
                child: TalkerScreen(
                  appBarTitle: 'Logs',
                  talker: logger,
                ),
              ),
            ],
          ),
        );
        _addCommandOutput('logs');

        return false;
      case 'admin':
        var result = await _adminApiService.getContractsStates();
        if (context.mounted)
          showDialog(
            context: context,
            builder: (_) => AdminPanelDialog(vm: result),
          );

        _addCommandOutput('admin');

        return false;
      case 'goto':
        _pageContext.goto(args!.first);
        _addCommandOutput('goto', args: args);
        return false;
      case 'set':
        // [s_withdrawalsEnabled|s_stopTheWorld] [true|false]
        var field = args!.elementAt(0);
        var value = bool.parse(args.elementAt(1));
        String txnHash;
        if (field == 's_withdrawalsEnabled') {
          txnHash = await _adminApiService.setWithdrawalsEnabled(value);
        } else if (field == 's_stopTheWorld') {
          txnHash = await _adminApiService.setStopTheWorld(value);
        } else {
          throw UnimplementedError();
        }

        _addCommandOutput('set', args: args, outputLines: [
          'Txn hash: $txnHash',
        ]);

        return true;
    }

    throw UnimplementedError();
  }
}
