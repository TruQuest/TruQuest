import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import 'local_wallet_from_imported_mnemonic_creation_dialog.dart';
import '../../user/bloc/user_actions.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';

class LocalWalletCreationDialog extends StatefulWidget {
  const LocalWalletCreationDialog({super.key});

  @override
  State<LocalWalletCreationDialog> createState() =>
      _LocalWalletCreationDialogState();
}

class _LocalWalletCreationDialogState
    extends StateX<LocalWalletCreationDialog> {
  late final _userBloc = use<UserBloc>();

  final _passwordController = TextEditingController();

  int _currentStep = 0;
  String? _mnemonic;

  @override
  void dispose() {
    _mnemonic = null;
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
            height: 300,
            child: Column(
              children: [
                Expanded(
                  // @@??: Do I need scrollable here?
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
                          child: Text(
                            details.currentStep == 0 ? 'Generate' : 'Create',
                          ),
                          onPressed: () async {
                            if (details.currentStep == 0) {
                              var action = GenerateMnemonic();
                              _userBloc.dispatch(action);

                              var success = await action.result;
                              if (success != null) {
                                _mnemonic = success.mnemonic;
                                details.onStepContinue!();
                              }
                            } else {
                              var action = CreateAndSaveEncryptedLocalWallet(
                                mnemonic: _mnemonic!,
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
                            'Generate a random secret phrase',
                            style: GoogleFonts.philosopher(
                              color: const Color(0xffF8F9FA),
                              fontSize: 16,
                            ),
                          ),
                          content: Container(
                            width: double.infinity,
                            padding: const EdgeInsets.only(bottom: 12),
                            child: Text(
                              '...',
                              style: GoogleFonts.raleway(
                                color: Colors.white,
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
                ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.white,
                    foregroundColor: Colors.black,
                  ),
                  child: const Text('Use an existing secret phrase instead'),
                  onPressed: () {
                    Navigator.of(context).pop();
                    showDialog(
                      context: context,
                      builder: (_) =>
                          const LocalWalletFromImportedMnemonicCreationDialog(),
                    );
                  },
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
