import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../ethereum/models/vm/smart_wallet.dart';
import '../../user/bloc/user_actions.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';

class SignInStepper extends StatefulWidget {
  const SignInStepper({super.key});

  @override
  State<SignInStepper> createState() => _SignInStepperState();
}

class _SignInStepperState extends StateX<SignInStepper> {
  late final _userBloc = use<UserBloc>();

  final _passwordController1 = TextEditingController();
  final _passwordController2 = TextEditingController();
  final _emailController = TextEditingController();
  final _confirmationTokenController = TextEditingController();

  late int _currentStep;
  SmartWallet? _wallet;

  @override
  void initState() {
    super.initState();
    _currentStep = 0;
  }

  @override
  void dispose() {
    _wallet = null;
    _passwordController1.dispose();
    _passwordController2.dispose();
    _emailController.dispose();
    _confirmationTokenController.dispose();
    super.dispose();
  }

  String _getButtonLabel(int step) {
    switch (step) {
      case 0:
        return 'Reserve';
      case 1:
        return 'Protect';
      case 2:
        return 'Sign-in';
      case 3:
        return 'Add';
      default:
        return 'Confirm';
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
        title: Text('Sign-in'),
        children: [
          SizedBox(
            width: 400,
            height: 600,
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
                      } else if (details.currentStep == 2) {
                        var action = SignInWithEthereum(
                          password: _passwordController2.text,
                        );
                        _userBloc.dispatch(action);

                        var success = await action.result;
                        if (success != null) {
                          _passwordController2.clear();
                          details.onStepContinue!();
                        }
                      } else if (details.currentStep == 3) {
                        var action = AddEmail(email: _emailController.text);
                        _userBloc.dispatch(action);

                        var success = await action.result;
                        if (success != null) {
                          _emailController.clear();
                          details.onStepContinue!();
                        }
                      } else {
                        var action = ConfirmEmail(
                          confirmationToken: _confirmationTokenController.text,
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
                        _wallet == null ? '...' : _wallet!.address,
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
                  Step(
                    title: Text(
                      'Add recovery email (optional but recommended)',
                      style: GoogleFonts.philosopher(
                        color: const Color(0xffF8F9FA),
                        fontSize: 16,
                      ),
                    ),
                    content: Padding(
                      padding: const EdgeInsets.only(bottom: 12),
                      child: TextField(
                        controller: _emailController,
                        decoration: const InputDecoration(
                          hintText: 'Email',
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
                      'Confirm email ownership',
                      style: GoogleFonts.philosopher(
                        color: const Color(0xffF8F9FA),
                        fontSize: 16,
                      ),
                    ),
                    content: Padding(
                      padding: const EdgeInsets.only(bottom: 12),
                      child: TextField(
                        controller: _confirmationTokenController,
                        decoration: const InputDecoration(
                          hintText: 'Confirmation token',
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
