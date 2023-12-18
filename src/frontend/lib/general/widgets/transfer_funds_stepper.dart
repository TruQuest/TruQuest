import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../user/bloc/user_actions.dart';
import '../utils/utils.dart';
import '../../user/bloc/user_bloc.dart';
import 'swipe_button.dart';
import '../../widget_extensions.dart';
import 'token_unit_switcher.dart';

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
  static const int _gtPerTru = 1000000000;

  late final _userBloc = use<UserBloc>();

  TokenUnit? _selectedUnit;
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
        controlsBuilder: (context, details) => SwipeButton.expand(
          height: 50,
          enabled: true,
          swiped: false,
          onTrackChild: Text(
            'Swipe to ${widget.direction == TransferDirection.deposit ? 'deposit' : 'withdraw'}',
            style: const TextStyle(color: Colors.black54),
          ),
          onExpandingHandleChild: const Icon(
            Icons.double_arrow_rounded,
            color: Colors.white,
          ),
          color: Colors.red,
          disabledColor: Colors.blue[200]!,
          trackColor: Colors.grey[350]!,
          onFullSwipe: () async {
            var value = _controller.text;
            int? amountInGt;
            if (_selectedUnit == TokenUnit.tru) {
              amountInGt = double.tryParse(value) != null ? (double.parse(value) * _gtPerTru).toInt() : null;
            } else {
              amountInGt = int.tryParse(value);
            }

            if (amountInGt == null) return false;

            bool success = await multiStageFlow(
              context,
              (ctx) => _userBloc.executeMultiStage(
                widget.direction == TransferDirection.deposit
                    ? DepositFunds(amount: amountInGt!)
                    : WithdrawFunds(amount: amountInGt!),
                ctx,
              ),
            );

            return success;
          },
        ),
        steps: [
          Step(
            title: Text(
              widget.direction == TransferDirection.deposit ? 'Deposit' : 'Withdraw',
              style: GoogleFonts.philosopher(
                color: const Color(0xffF8F9FA),
                fontSize: 20,
              ),
            ),
            subtitle: Text(
              '1 TRU = 1 000 000 000 GT (Guttae [guht-ee])',
              style: GoogleFonts.philosopher(
                color: Colors.white,
                fontSize: 15,
              ),
            ),
            content: Container(
              height: 68,
              padding: const EdgeInsets.only(bottom: 20),
              child: Row(
                children: [
                  Expanded(
                    child: TextField(
                      controller: _controller,
                      keyboardType: TextInputType.number,
                      expands: true,
                      maxLines: null,
                      decoration: const InputDecoration(
                        contentPadding: EdgeInsets.fromLTRB(6, 16, 12, 16),
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
                  const SizedBox(width: 8),
                  TokenUnitSwitcher(
                    onUnitSwitched: (selectedUnit) {
                      if (_selectedUnit == null) {
                        _selectedUnit = selectedUnit;
                        return;
                      }
                      if (_selectedUnit == selectedUnit) return;

                      var value = _controller.text;
                      if (_selectedUnit == TokenUnit.tru) {
                        var amount = double.tryParse(value);
                        if (amount != null) {
                          amount *= _gtPerTru;
                          _controller.text = amount.toInt().toString();
                        } else {
                          _controller.text = '0';
                        }
                      } else if (_selectedUnit == TokenUnit.gt) {
                        var amount = double.tryParse(value);
                        if (amount != null) {
                          _controller.text = (amount.toInt().toDouble() / _gtPerTru).toStringAsFixed(9).trimZeros();
                        } else {
                          _controller.text = '0';
                        }
                      }

                      _selectedUnit = selectedUnit;
                    },
                  ),
                ],
              ),
            ),
            isActive: true,
          ),
        ],
      ),
    );
  }
}
