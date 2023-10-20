import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../utils/utils.dart';
import '../../user/bloc/user_actions.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';

class OnboardingDialog extends StatefulWidget {
  const OnboardingDialog({super.key});

  @override
  State<OnboardingDialog> createState() => _OnboardingDialogState();
}

class _OnboardingDialogState extends StateX<OnboardingDialog> {
  late final _userBloc = use<UserBloc>();

  int _currentStep = 0;
  final _emailController = TextEditingController();
  final _confirmationCodeController = TextEditingController();

  String? _iframeViewId;

  @override
  void dispose() {
    _emailController.dispose();
    _confirmationCodeController.dispose();
    super.dispose();
  }

  @override
  Widget buildX(BuildContext context) {
    return Theme(
      data: getThemeDataForSteppers(context),
      child: SimpleDialog(
        backgroundColor: const Color(0xFF242423),
        title: Text(
          'Sign-up',
          style: GoogleFonts.philosopher(
            color: Colors.white,
            fontSize: 24,
          ),
        ),
        children: [
          SizedBox(
            width: 400,
            height: 320,
            child: Stepper(
              currentStep: _currentStep,
              onStepContinue: () => setState(() => _currentStep++),
              controlsBuilder: (context, details) => Align(
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
                      details.currentStep == 0
                          ? 'Sign-up'
                          : details.currentStep == 1
                              ? 'Confirm'
                              : 'Save',
                    ),
                    onPressed: () async {
                      if (details.currentStep == 0) {
                        var success = await _userBloc.execute<bool>(
                          SignUp(email: _emailController.text),
                        );
                        if (success.isTrue) details.onStepContinue!();
                      } else {
                        _iframeViewId = await _userBloc.execute<String>(
                          FinishSignUp(
                            email: _emailController.text,
                            confirmationCode: _confirmationCodeController.text,
                          ),
                        );
                        if (_iframeViewId != null) details.onStepContinue!();
                      }
                    },
                  ),
                ),
              ),
              steps: [
                Step(
                  title: Text(
                    'Enter your email',
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
                    'Enter confirmation code',
                    style: GoogleFonts.philosopher(
                      color: const Color(0xffF8F9FA),
                      fontSize: 16,
                    ),
                  ),
                  content: Padding(
                    padding: const EdgeInsets.only(bottom: 12),
                    child: TextField(
                      controller: _confirmationCodeController,
                      decoration: const InputDecoration(
                        hintText: '6-digit code',
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
                    'Please save the QR-code on your device',
                    style: GoogleFonts.philosopher(
                      color: const Color(0xffF8F9FA),
                      fontSize: 16,
                    ),
                  ),
                  content: Container(
                    width: 300,
                    height: 300,
                    padding: const EdgeInsets.only(bottom: 12),
                    child: HtmlElementView(viewType: _userBloc.privateKeyGenIframeViewId),
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
