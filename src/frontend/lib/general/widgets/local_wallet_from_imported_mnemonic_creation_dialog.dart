import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../user/bloc/user_actions.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';

class LocalWalletFromImportedMnemonicCreationDialog extends StatefulWidget {
  const LocalWalletFromImportedMnemonicCreationDialog({super.key});

  @override
  State<LocalWalletFromImportedMnemonicCreationDialog> createState() =>
      _LocalWalletFromImportedMnemonicCreationDialogState();
}

class _LocalWalletFromImportedMnemonicCreationDialogState
    extends StateX<LocalWalletFromImportedMnemonicCreationDialog> {
  late final _userBloc = use<UserBloc>();

  final _mnemonicController = TextEditingController();
  final _passwordController = TextEditingController();

  int _currentStep = 0;

  @override
  void dispose() {
    _mnemonicController.dispose();
    _passwordController.dispose();
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
      child: SimpleDialog(
        backgroundColor: const Color(0xFF242423),
        children: [
          SizedBox(
            width: 400,
            height: 400,
            child: SingleChildScrollView(
              child: Stepper(
                currentStep: _currentStep,
                onStepContinue: () => setState(() => _currentStep++),
                controlsBuilder: (context, details) {
                  return ElevatedButton(
                    style: ElevatedButton.styleFrom(
                      backgroundColor: const Color(0xFF242423),
                      foregroundColor: Colors.white,
                    ),
                    child: Text(details.currentStep == 0 ? 'Import' : 'Create'),
                    onPressed: () async {
                      if (details.currentStep == 0) {
                        details.onStepContinue!();
                      } else {
                        var action = CreateAndSaveEncryptedLocalWallet(
                          mnemonic: _mnemonicController.text,
                          password: _passwordController.text,
                        );
                        _userBloc.dispatch(action);

                        var success = await action.result;
                        if (success != null) {
                          if (context.mounted) {
                            Navigator.of(context).pop();
                          }
                        }
                      }
                    },
                  );
                },
                steps: [
                  Step(
                    title: Text(
                      'Import a secret phrase',
                      style: GoogleFonts.philosopher(
                        color: const Color(0xffF8F9FA),
                        fontSize: 16,
                      ),
                    ),
                    content: Container(
                      height: 100,
                      padding: const EdgeInsets.only(bottom: 12),
                      child: TextField(
                        controller: _mnemonicController,
                        expands: true,
                        minLines: null,
                        maxLines: null,
                        decoration: const InputDecoration(
                          hintText: 'Mnemonic',
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
                      'Create a wallet from the phrase and a password',
                      style: GoogleFonts.philosopher(
                        color: const Color(0xffF8F9FA),
                        fontSize: 16,
                      ),
                    ),
                    content: Padding(
                      padding: const EdgeInsets.only(bottom: 12),
                      child: TextField(
                        controller: _passwordController,
                        obscureText: true,
                        decoration: const InputDecoration(
                          hintText: 'Password',
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
            ),
          ),
        ],
      ),
    );
  }
}
