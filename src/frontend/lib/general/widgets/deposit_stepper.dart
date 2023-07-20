import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../user/errors/wallet_locked_error.dart';
import '../../user/bloc/user_actions.dart';
import '../../user/bloc/user_bloc.dart';
import 'swipe_button.dart';
import '../../widget_extensions.dart';
import 'unlock_wallet_dialog.dart';

class DepositStepper extends StatefulWidget {
  const DepositStepper({super.key});

  @override
  State<DepositStepper> createState() => _DepositStepperState();
}

class _DepositStepperState extends StateX<DepositStepper> {
  late final _userBloc = use<UserBloc>();

  final _approveController = TextEditingController();
  final _depositController = TextEditingController();

  int _currentStep = 0;

  @override
  void initState() {
    super.initState();
    _approveController.addListener(() {
      _depositController.text = _approveController.text;
    });
  }

  @override
  void dispose() {
    _approveController.dispose();
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
        currentStep: _currentStep,
        controlsBuilder: (context, details) => SwipeButton(
          key: ValueKey(details.currentStep),
          text: 'Swipe to ${details.currentStep == 0 ? 'approve' : 'deposit'}',
          enabled: true,
          swiped: false,
          onCompletedSwipe: () async {
            if (details.currentStep == 0) {
              var action = ApproveFundsUsage(
                amount: int.parse(_approveController.text),
              );
              _userBloc.dispatch(action);

              var failure = await action.result;
              if (failure == null) {
                details.onStepContinue!();
              }

              return failure == null;
            }

            var amountToDeposit = int.parse(_depositController.text);
            var action = DepositFunds(amount: amountToDeposit);
            _userBloc.dispatch(action);

            var failure = await action.result;
            if (failure != null && failure.error is WalletLockedError) {
              if (context.mounted) {
                var unlocked = await showDialog<bool>(
                  context: context,
                  barrierDismissible: false,
                  builder: (_) => UnlockWalletDialog(),
                );

                if (unlocked != null && unlocked) {
                  action = DepositFunds(amount: amountToDeposit);
                  _userBloc.dispatch(action);

                  failure = await action.result;
                }
              }
            }

            return failure == null;
          },
        ),
        onStepContinue: () => setState(() => _currentStep = 1),
        onStepTapped: (step) => setState(() => _currentStep = step),
        steps: [
          Step(
            title: Text(
              'Approve',
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
                controller: _approveController,
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
