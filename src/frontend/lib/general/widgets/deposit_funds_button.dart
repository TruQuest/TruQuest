import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import 'restrict_when_unauthorized_button.dart';
import 'transfer_funds_stepper.dart';

class DepositFundsButton extends StatelessWidget {
  const DepositFundsButton({super.key});

  @override
  Widget build(BuildContext context) {
    return RestrictWhenUnauthorizedButton(
      child: ElevatedButton(
        style: ElevatedButton.styleFrom(
          fixedSize: const Size.fromWidth(100),
          backgroundColor: Colors.white,
          foregroundColor: Colors.black,
          textStyle: GoogleFonts.righteous(fontSize: 12),
        ),
        child: const Text('Deposit'),
        onPressed: () => showDialog(
          context: context,
          builder: (context) => SimpleDialog(
            backgroundColor: const Color(0xFF242423),
            title: Text(
              'Deposit Truthserum to be used on the platform',
              style: GoogleFonts.philosopher(color: Colors.white),
            ),
            contentPadding: const EdgeInsets.fromLTRB(20, 24, 20, 12),
            children: [
              Container(
                decoration: BoxDecoration(
                  border: Border.all(color: Colors.white),
                ),
                width: 300,
                padding: const EdgeInsets.all(12),
                child: Text(
                  'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor. '
                  'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor',
                  style: GoogleFonts.raleway(color: Colors.white),
                ),
              ),
              const SizedBox(height: 16),
              const SizedBox(
                width: 300,
                child: TransferFundsStepper(direction: TransferDirection.deposit),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
