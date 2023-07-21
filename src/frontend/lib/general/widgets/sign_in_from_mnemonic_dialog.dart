import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../user/bloc/user_actions.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';

class SignInFromMnemonicDialog extends StatefulWidget {
  const SignInFromMnemonicDialog({super.key});

  @override
  State<SignInFromMnemonicDialog> createState() =>
      _SignInFromMnemonicDialogState();
}

class _SignInFromMnemonicDialogState extends StateX<SignInFromMnemonicDialog> {
  late final _userBloc = use<UserBloc>();

  final _mnemonicController = TextEditingController();
  final _passwordController1 = TextEditingController();
  final _passwordController2 = TextEditingController();

  late int _currentStep;

  @override
  void initState() {
    super.initState();
    var currentUser = _userBloc.latestCurrentUser!;
    if (currentUser.walletAddress == null) {
      _currentStep = 0;
    } else {
      _currentStep = 2;
    }
  }

  @override
  void dispose() {
    _mnemonicController.dispose();
    _passwordController1.dispose();
    _passwordController2.dispose();
    super.dispose();
  }

  String _getButtonLabel(int step) {
    switch (step) {
      case 0:
        return 'Import';
      case 1:
        return 'Create';
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
            height: 500,
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
                        details.onStepContinue!();
                      } else if (details.currentStep == 1) {
                        var action = CreateAndSaveEncryptedSmartWallet(
                          mnemonic: _mnemonicController.text,
                          password: _passwordController1.text,
                        );
                        _userBloc.dispatch(action);

                        var success = await action.result;
                        if (success != null) {
                          _mnemonicController.clear();
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
                      'Create a smart wallet from the phrase and a password',
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
        ],
      ),
    );
  }
}
