import 'package:flutter/material.dart';
import 'package:email_validator/email_validator.dart';
import 'package:rxdart/rxdart.dart';
import 'package:talker_flutter/talker_flutter.dart';

import '../../ethereum_js_interop.dart';
import '../../general/contexts/page_context.dart';
import '../../general/utils/logger.dart';
import '../models/im/whitelist_entry_type_im.dart';
import '../models/vm/command_output.dart';
import '../widgets/admin_panel_dialog.dart';
import 'admin_api_service.dart';

class AdminService {
  static const _validCommands = [
    'help',
    'cls',
    'exit',
    'logs',
    'admin',
    'goto',
    'set',
    'whitelist',
    'find_user_by_email',
    'access',
    'fund',
  ];

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

  void _clearCommandOutputs() {
    _commandOutputs.clear();
    _commandOutputsChannel.add(_commandOutputs);
  }

  String? _validate(String command, List<String>? args) {
    if (!_validCommands.contains(command)) {
      return 'Invalid command';
    }

    switch (command) {
      case 'help':
      case 'cls':
      case 'exit':
      case 'logs':
      case 'admin':
        if (args != null) return 'This command doesn\'t take any arguments';
      case 'goto':
        if (args == null || args.length != 1) return 'This command takes exactly 1 argument';
      case 'set':
        if (args == null || args.length != 2) return 'This command takes exactly 2 arguments';
        if (args.first != 's_withdrawalsEnabled' && args.first != 's_stopTheWorld')
          return "First argument can be either 's_withdrawalsEnabled' or 's_stopTheWorld'";
        if (args.last != 'true' && args.last != 'false') return "Second argument can be either 'true' or 'false'";
      case 'whitelist':
        if (args == null || args.length != 3) return 'This command takes exactly 3 arguments';
        if (args.first != 'add' && args.first != 'remove') return "First argument can be either 'add' or 'remove'";
        if (args[1] != 'email' && args[1] != 'signer_address')
          return "Second argument can be either 'email' or 'signer_address'";
        if (args[1] == 'email' && !EmailValidator.validate(args.last)) return 'Invalid email';
        if (args[1] == 'signer_address' && !isValidAddress(args.last)) return 'Invalid signer address';
      case 'find_user_by_email':
        if (args == null || args.length != 1) return 'This command takes exactly 1 argument';
        if (!EmailValidator.validate(args.first)) return 'Invalid email';
      case 'access':
        if (args == null || args.length != 2) return 'This command takes exactly 2 arguments';
        if (args.first != 'give' && args.first != 'remove') return "First argument can be either 'give' or 'remove'";
        if (args.first == 'give' &&
            (args.last.split(',').isEmpty || args.last.split(',').any((a) => !isValidAddress(a))))
          return 'Second argument must be a list of comma-separated addresses';
        if (args.first == 'remove' && !isValidAddress(args.last)) return 'Second argument must an address';
      case 'fund':
        if (args == null || args.length != 3) return 'This command takes exactly 3 arguments';
        if (!isValidAddress(args.first)) return 'Invalid address';
        var arg2Split = args[1].split(':');
        if (arg2Split.length != 2 || arg2Split.first != 'ETH' || double.tryParse(arg2Split.last) == null)
          return "Second argument must be of the form 'ETH:<AMOUNT_IN_ETH>'";
        var arg3Split = args[2].split(':');
        if (arg3Split.length != 2 || arg3Split.first != 'TRU' || double.tryParse(arg3Split.last) == null)
          return "Third argument must be of the form 'TRU:<AMOUNT_IN_TRU>'";
    }

    return null;
  }

  Future<bool> command(String command, List<String>? args, BuildContext context) async {
    String? error = _validate(command, args);
    if (error != null) {
      _addCommandOutput(command, args: args, outputLines: ['Error: $error']);
      return true;
    }

    try {
      switch (command) {
        case 'help':
          _addCommandOutput('help', outputLines: _validCommands);
          return true;
        case 'cls':
          _clearCommandOutputs();
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
          var field = args!.first;
          var value = bool.parse(args.last);
          String txnHash;
          if (field == 's_withdrawalsEnabled') {
            txnHash = await _adminApiService.setWithdrawalsEnabled(value);
          } else {
            txnHash = await _adminApiService.setStopTheWorld(value);
          }

          _addCommandOutput('set', args: args, outputLines: [
            'Txn hash: $txnHash',
          ]);

          return true;
        case 'whitelist':
          var action = args!.first;
          var entryType = WhitelistEntryTypeImExtension.fromString(args[1]);
          var entry = entryType == WhitelistEntryTypeIm.signerAddress ? convertToEip55Address(args.last) : args.last;
          if (action == 'add') {
            await _adminApiService.addToWhitelist(entryType, entry);
          } else {
            await _adminApiService.removeFromWhitelist(entryType, entry);
          }

          _addCommandOutput('whitelist', args: args, outputLines: ['Success!']);

          return true;
        case 'find_user_by_email':
          var email = args!.first;
          var user = await _adminApiService.getUserByEmail(email);
          _addCommandOutput('find_user_by_email', args: args, outputLines: [
            'UserId: ${user.userId}',
            'WalletAddress: ${user.walletAddress}',
            'SignerAddress: ${user.signerAddress}',
            'EmailConfirmed: ${user.emailConfirmed}',
          ]);

          return true;
        case 'access':
          var action = args!.first;
          String txnHash;
          if (action == 'give') {
            var addresses = args.last.split(',');
            txnHash = await _adminApiService.giveAccessTo(addresses.map((a) => convertToEip55Address(a)).toList());
          } else {
            var address = args.last;
            txnHash = await _adminApiService.removeAccessFrom(convertToEip55Address(address));
          }

          _addCommandOutput('access', args: args, outputLines: [
            'Txn hash: $txnHash',
          ]);

          return true;
        case 'fund':
          var address = args!.first;
          var amountInEth = double.parse(args[1].split(':').last);
          var amountInTru = double.parse(args[2].split(':').last);

          var result = await _adminApiService.fundWithEthAndTru(address, amountInEth, amountInTru);

          _addCommandOutput('fund', args: args, outputLines: [
            'Fund with Eth txn hash: ${result.ethTxnHash}',
            'Fund with Tru txn hash: ${result.truTxnHash}',
          ]);

          return true;
      }
    } catch (e) {
      _addCommandOutput(command, args: args, outputLines: ["Error executing command '$command': $e"]);
      return true;
    }

    throw UnimplementedError();
  }
}
