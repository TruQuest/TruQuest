import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import 'sign_in_from_mnemonic_dialog.dart';
import '../../ethereum/models/vm/smart_wallet.dart';
import '../../user/bloc/user_actions.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';

class SignInDialog extends StatefulWidget {
  const SignInDialog({super.key});

  @override
  State<SignInDialog> createState() => _SignInDialogState();
}

class _SignInDialogState extends StateX<SignInDialog> {
  late final _userBloc = use<UserBloc>();

  final _passwordController1 = TextEditingController();
  final _passwordController2 = TextEditingController();

  late int _currentStep;
  SmartWallet? _wallet;

  @override
  void initState() {
    super.initState();
    var currentUser = _userBloc.latestCurrentUser!.user;
    if (currentUser.walletAddress == null) {
      _currentStep = 0;
    } else {
      _currentStep = 2;
    }

    // @@NOTE: If user clicks Sign-in but then closes the dialog and opens it up again
    // while the sign-in process is still in progress, he will be directed to the step = 2
    // and won't receive an update when the process gets completed.
    // This is fine though since signing-in multiple times is ok.
  }

  @override
  void dispose() {
    _wallet = null;
    _passwordController1.dispose();
    _passwordController2.dispose();
    super.dispose();
  }

  String _getButtonLabel(int step) {
    switch (step) {
      case 0:
        return 'Reserve';
      case 1:
        return 'Protect';
      default:
        return 'Sign-in';
    }
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
                          child: Text(_getButtonLabel(details.currentStep)),
                          onPressed: () async {
                            if (details.currentStep == 0) {
                              var action = CreateSmartWallet();
                              _userBloc.dispatch(action);

                              var success = await action.result;
                              if (success != null) {
                                _wallet = success.wallet;
                                details.onStepContinue!();
                              }
                            } else if (details.currentStep == 1) {
                              var action = EncryptAndSaveSmartWallet(
                                wallet: _wallet!,
                                password: _passwordController1.text,
                              );
                              _userBloc.dispatch(action);

                              var success = await action.result;
                              if (success != null) {
                                _wallet = null;
                                _passwordController1.clear();
                                details.onStepContinue!();
                              }
                            } else {
                              var action = SignInWithEthereum(
                                password: _passwordController2.text,
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
                            'Reserve a smart wallet address',
                            style: GoogleFonts.philosopher(
                              color: const Color(0xffF8F9FA),
                              fontSize: 16,
                            ),
                          ),
                          content: Container(
                            width: double.infinity,
                            padding: const EdgeInsets.only(bottom: 12),
                            child: Text(
                              _wallet == null
                                  ? '...'
                                  : _wallet!.currentWalletAddress,
                              style: GoogleFonts.raleway(
                                color: Colors.white,
                              ),
                            ),
                          ),
                          isActive: true,
                        ),
                        Step(
                          title: Text(
                            'Password-protect the access to the smart wallet',
                            style: GoogleFonts.philosopher(
                              color: const Color(0xffF8F9FA),
                              fontSize: 16,
                            ),
                          ),
                          content: Padding(
                            padding: const EdgeInsets.only(bottom: 12),
                            child: TextField(
                              controller: _passwordController1,
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
                        Step(
                          title: Text(
                            'Sign-in',
                            style: GoogleFonts.philosopher(
                              color: const Color(0xffF8F9FA),
                              fontSize: 16,
                            ),
                          ),
                          content: Padding(
                            padding: const EdgeInsets.only(bottom: 12),
                            child: TextField(
                              controller: _passwordController2,
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
                  child: Text('Import a mnemonic instead'),
                  onPressed: () {
                    Navigator.of(context).pop();
                    showDialog(
                      context: context,
                      builder: (_) => SignInFromMnemonicDialog(),
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
