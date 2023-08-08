import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../utils/utils.dart';
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
      data: getThemeDataForSteppers(context),
      child: SimpleDialog(
        backgroundColor: const Color(0xFF242423),
        title: Text(
          'Create a wallet',
          style: GoogleFonts.philosopher(
            color: Colors.white,
            fontSize: 24,
          ),
        ),
        children: [
          SizedBox(
            width: 400,
            height: 300,
            child: Stepper(
              currentStep: _currentStep,
              onStepContinue: () => setState(() => _currentStep++),
              controlsBuilder: (context, details) {
                return Align(
                  alignment: Alignment.centerLeft,
                  child: Padding(
                    padding: const EdgeInsets.only(top: 8),
                    child: ElevatedButton(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.white,
                        foregroundColor: const Color(0xFF242423),
                        padding: const EdgeInsets.fromLTRB(16, 8, 16, 8),
                      ),
                      child: Text(details.currentStep == 0 ? 'Import' : 'Create'),
                      onPressed: () async {
                        if (details.currentStep == 0) {
                          details.onStepContinue!();
                        } else {
                          var success = await _userBloc.execute<bool>(
                            CreateAndSaveEncryptedLocalWallet(
                              mnemonic: _mnemonicController.text,
                              password: _passwordController.text,
                            ),
                          );

                          if (success.isTrue) {
                            if (context.mounted) Navigator.of(context).pop();
                          }
                        }
                      },
                    ),
                  ),
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
        ],
      ),
    );
  }
}
