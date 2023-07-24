import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../errors/insufficient_balance_error.dart';
import '../../ethereum/errors/user_operation_error.dart';
import '../../ethereum/models/im/user_operation.dart';
import '../contexts/multi_stage_operation_context.dart';
import '../utils/utils.dart';
import '../../user/errors/wallet_locked_error.dart';
import '../../user/bloc/user_bloc.dart';
import 'swipe_button.dart';
import '../../widget_extensions.dart';

class DepositStepper extends StatefulWidget {
  const DepositStepper({super.key});

  @override
  State<DepositStepper> createState() => _DepositStepperState();
}

class _DepositStepperState extends StateX<DepositStepper> {
  late final _userBloc = use<UserBloc>();

  final _depositController = TextEditingController();

  @override
  void dispose() {
    _depositController.dispose();
    super.dispose();
  }

  @override
  Widget buildX(BuildContext context) {
    return Theme(
      data: ThemeData(
        brightness: Brightness.dark,
        colorScheme: Theme.of(context).colorScheme.copyWith(
              brightness: Brightness.dark,
              secondary: const Color(0xffF8F9FA),
            ),
      ),
      child: Stepper(
        currentStep: 0,
        controlsBuilder: (context, details) => SwipeButton(
          text: 'Swipe to deposit',
          enabled: true,
          swiped: false,
          onCompletedSwipe: () async {
            var ctx = MultiStageOperationContext();
            await for (var stageResult in _userBloc.depositFunds(
              int.parse(_depositController.text),
              ctx,
            )) {
              if (stageResult is WalletLockedError) {
                bool unlocked = false;
                if (context.mounted) {
                  unlocked = await showUnlockWalletDialog(context);
                }
                ctx.unlockWalletTask.complete(unlocked);
              } else if (stageResult is InsufficientBalanceError) {
                // ...
              } else if (stageResult is Stream<UserOperation>) {
                UserOperation? userOp;
                if (context.mounted) {
                  userOp = await showUserOpDialog(context, stageResult);
                }
                ctx.approveUserOpTask.complete(userOp);
              } else if (stageResult is UserOperationError) {
                // ...
              }
            }

            return false;
          },
        ),
        steps: [
          // @@TODO: Allow specifying units (drops?).
          Step(
            title: Text(
              'Deposit',
              style: GoogleFonts.philosopher(
                color: const Color(0xffF8F9FA),
                fontSize: 16,
              ),
            ),
            subtitle: Text(
              'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor',
              style: GoogleFonts.raleway(
                color: Colors.white,
              ),
            ),
            content: Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: TextField(
                controller: _depositController,
                keyboardType: TextInputType.number,
                decoration: const InputDecoration(
                  hintText: 'Amount',
                  hintStyle: TextStyle(color: Colors.white70),
                  enabledBorder: UnderlineInputBorder(
                    borderSide: BorderSide(color: Colors.white70),
                  ),
                  focusedBorder: UnderlineInputBorder(
                    borderSide: BorderSide(color: Colors.white),
                  ),
                ),
              ),
            ),
            isActive: true,
          ),
        ],
      ),
    );
  }
}
