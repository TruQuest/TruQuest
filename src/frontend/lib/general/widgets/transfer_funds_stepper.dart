import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../user/bloc/user_actions.dart';
import '../utils/utils.dart';
import '../../user/bloc/user_bloc.dart';
import 'swipe_button.dart';
import '../../widget_extensions.dart';

enum TransferDirection {
  deposit,
  withdraw,
}

class TransferFundsStepper extends StatefulWidget {
  final TransferDirection direction;

  const TransferFundsStepper({super.key, required this.direction});

  @override
  State<TransferFundsStepper> createState() => _TransferFundsStepperState();
}

class _TransferFundsStepperState extends StateX<TransferFundsStepper> {
  late final _userBloc = use<UserBloc>();

  final _controller = TextEditingController();

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget buildX(BuildContext context) {
    return Theme(
      data: getThemeDataForSteppers(context),
      child: Stepper(
        currentStep: 0,
        controlsBuilder: (context, details) => SwipeButton(
          text: 'Swipe to ${widget.direction == TransferDirection.deposit ? 'deposit' : 'withdraw'}',
          enabled: true,
          swiped: false,
          onCompletedSwipe: () async {
            bool success = await multiStageFlow(
              context,
              (ctx) => _userBloc.executeMultiStage(
                widget.direction == TransferDirection.deposit
                    ? DepositFunds(amount: int.tryParse(_controller.text) ?? 0)
                    : WithdrawFunds(amount: int.tryParse(_controller.text) ?? 0),
                ctx,
              ),
            );

            return success;
          },
        ),
        steps: [
          // @@TODO: Allow specifying in both TRU and GT.
          Step(
            title: Text(
              widget.direction == TransferDirection.deposit ? 'Deposit' : 'Withdraw',
              style: GoogleFonts.philosopher(
                color: const Color(0xffF8F9FA),
                fontSize: 20,
              ),
            ),
            subtitle: Text(
              'Specify amount in Guttae /guht-ee/\n1 TRU = 1 000 000 000 GT',
              style: GoogleFonts.raleway(
                color: Colors.white,
                fontSize: 14,
                height: 1.3,
              ),
            ),
            content: Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: TextField(
                controller: _controller,
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
