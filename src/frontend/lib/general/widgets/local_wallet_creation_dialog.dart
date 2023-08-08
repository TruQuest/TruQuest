import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import 'local_wallet_from_imported_mnemonic_creation_dialog.dart';
import '../utils/utils.dart';
import '../../user/bloc/user_actions.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';

class LocalWalletCreationDialog extends StatefulWidget {
  const LocalWalletCreationDialog({super.key});

  @override
  State<LocalWalletCreationDialog> createState() => _LocalWalletCreationDialogState();
}

class _LocalWalletCreationDialogState extends StateX<LocalWalletCreationDialog> {
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
      data: getThemeDataForSteppers(context),
      child: SimpleDialog(
        backgroundColor: const Color(0xFF242423),
        title: SizedBox(
          width: 400,
          child: Row(
            children: [
              Text(
                'Create a wallet',
                style: GoogleFonts.philosopher(
                  color: Colors.white,
                  fontSize: 24,
                ),
              ),
              Spacer(),
              ElevatedButton.icon(
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.white,
                  foregroundColor: Colors.black,
                ),
                icon: Icon(
                  Icons.text_snippet_outlined,
                  size: 16,
                ),
                label: const Text(
                  'Use an existing phrase instead',
                  style: TextStyle(fontSize: 12),
                ),
                onPressed: () {
                  Navigator.of(context).pop();
                  showDialog(
                    context: context,
                    builder: (_) => const LocalWalletFromImportedMnemonicCreationDialog(),
                  );
                },
              ),
            ],
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
                      child: Text(
                        details.currentStep == 0 ? 'Generate' : 'Create',
                      ),
                      onPressed: () async {
                        if (details.currentStep == 0) {
                          _mnemonic = await _userBloc.execute<String>(
                            const GenerateMnemonic(),
                          );
                          details.onStepContinue!();
                        } else {
                          var success = await _userBloc.execute<bool>(
                            CreateAndSaveEncryptedLocalWallet(
                              mnemonic: _mnemonic!,
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
                      'Secret phrase in combination with a password gives control over the wallet',
                      style: GoogleFonts.raleway(
                        color: Colors.white70,
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
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Text(
                          _mnemonic ?? '',
                          style: GoogleFonts.raleway(
                            color: Colors.white70,
                          ),
                        ),
                        SizedBox(height: 12),
                        TextField(
                          controller: _passwordController,
                          obscureText: true,
                          decoration: const InputDecoration(
                            hintText: 'Password',
                            hintStyle: TextStyle(color: Colors.white),
                            enabledBorder: UnderlineInputBorder(
                              borderSide: BorderSide(color: Colors.white70),
                            ),
                            focusedBorder: UnderlineInputBorder(
                              borderSide: BorderSide(color: Colors.white),
                            ),
                          ),
                        ),
                      ],
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
