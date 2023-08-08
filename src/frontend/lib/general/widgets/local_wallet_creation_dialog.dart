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
            height: 320,
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
                      child: Text(details.currentStep == 0 ? 'Generate' : 'Create'),
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
                      'Secret phrase in combination with a password gives control over the wallet.',
                      style: GoogleFonts.raleway(
                        color: Colors.white70,
                      ),
                    ),
                  ),
                  isActive: true,
                ),
                Step(
                  title: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(
                        'Create a wallet from the phrase and a password',
                        style: GoogleFonts.philosopher(
                          color: const Color(0xffF8F9FA),
                          fontSize: 16,
                        ),
                      ),
                      SizedBox(width: 8),
                      Tooltip(
                        message: 'The Secret Phrase in combination with the provided password controls your wallet.\n'
                            'That is, to generate keys that authorize you to access the wallet you need both the phrase\n'
                            'and the password. The phrase by itself is not enough (technical note: this is achieved\n'
                            'using the BIP-39 passphrase specification).\n\n'
                            'This means that as long as you keep the phrase and the password in two different places\n'
                            '(e.g. the phrase on a piece of paper and the password in a password manager)\n'
                            'you essentially have two-factor authentication. Provided that you selected a strong password,\n'
                            'accidentally revealing the secret phrase wouldn\'t compromise the wallet.\n\n'
                            'Most other wallets (e.g. Metamask) use only the phrase to generate keys, which makes it\n'
                            'dangerous to back it up in a place that could potentially be compromised (e.g. you keep the phrase\n'
                            'on a piece of paper in your house and it gets robbed, or you keep the phrase on your phone and then\n'
                            'lose it). By making authorization two-factor we avoid this problem.',
                        textStyle: GoogleFonts.philosopher(),
                        child: Icon(Icons.info),
                      ),
                    ],
                  ),
                  content: Padding(
                    padding: const EdgeInsets.only(bottom: 12),
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Secret phrase (back it up):',
                          style: GoogleFonts.philosopher(
                            color: Colors.white,
                            fontSize: 18,
                          ),
                        ),
                        SizedBox(height: 6),
                        SelectableText(
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
                            hintStyle: TextStyle(color: Colors.white70),
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
