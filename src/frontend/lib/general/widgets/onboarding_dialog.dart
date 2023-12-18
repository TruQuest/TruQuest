import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../ethereum_js_interop.dart';
import '../services/iframe_manager.dart';
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
  late final _iframeManager = use<IFrameManager>();
  late final _userBloc = use<UserBloc>();

  int _currentStep = 0;
  final _emailController = TextEditingController();
  final _confirmationCodeController = TextEditingController();

  bool _signInMode = false;

  AttestationOptions? _options;

  @override
  void dispose() {
    _emailController.dispose();
    _confirmationCodeController.dispose();
    super.dispose();
  }

  // Widget _buildThirdPartyWalletButton(String walletName) {
  //   return InkWell(
  //     onTap: () => multiStageOffChainFlow(
  //       context,
  //       (ctx) => _userBloc.executeMultiStage(
  //         SignInWithThirdPartyWallet(walletName: walletName),
  //         ctx,
  //       ),
  //     ),
  //     child: Card(
  //       color: const Color(0xffF8F9FA),
  //       shadowColor: Colors.white,
  //       shape: RoundedRectangleBorder(
  //         borderRadius: BorderRadius.circular(8),
  //       ),
  //       elevation: 5,
  //       child: Padding(
  //         padding: const EdgeInsets.all(8),
  //         child: Image.asset(
  //           'assets/images/${walletName.toLowerCase()}.png',
  //           width: 300,
  //           height: 50,
  //           fit: BoxFit.contain,
  //         ),
  //       ),
  //     ),
  //   );
  // }

  @override
  Widget buildX(BuildContext context) {
    return SimpleDialog(
      backgroundColor: const Color(0xFF242423),
      title: SizedBox(
        width: 400,
        child: SwitchListTile(
          value: _signInMode,
          onChanged: (value) => setState(() => _signInMode = value),
          activeColor: Colors.blueAccent[700],
          inactiveThumbColor: Colors.blueAccent[700],
          activeTrackColor: Colors.white,
          inactiveTrackColor: Colors.white,
          title: Text(
            _signInMode ? 'Sign-in' : 'Sign-up',
            style: GoogleFonts.philosopher(
              color: Colors.white,
              fontSize: 24,
            ),
          ),
          subtitle: Text(
            'Sign-up / Sign-in',
            style: GoogleFonts.raleway(
              color: Colors.white.withOpacity(0.6),
              fontSize: 16,
            ),
          ),
        ),
      ),
      children: [
        SizedBox(
          width: 400,
          height: _signInMode ? 200 : 350,
          child: _signInMode
              ? Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 24),
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      ListTile(
                        tileColor: Colors.white,
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(8),
                        ),
                        leading: const Icon(Icons.devices),
                        title: const Text('Signed-in from this device before'),
                        onTap: () async {
                          var success = await multiStageOffChainFlow(
                            context,
                            (ctx) => _userBloc.executeMultiStage(
                              const SignInFromExistingDevice(),
                              ctx,
                            ),
                          );
                          if (success && context.mounted) Navigator.of(context).pop();
                        },
                      ),
                      const SizedBox(height: 16),
                      ListTile(
                        tileColor: Colors.white,
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(8),
                        ),
                        leading: const Icon(Icons.device_unknown),
                        title: const Text('First time from this device'),
                        subtitle: const Text(
                          'Scan a QR-code from an already signed-in device',
                        ), // @@TODO: Help message.
                        onTap: () {},
                      ),
                    ],
                  ),
                )
              : Theme(
                  data: getThemeDataForSteppers(context),
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
                              _options = await _userBloc.execute<AttestationOptions>(
                                GenerateConfirmationCodeAndAttestationOptions(email: _emailController.text),
                              );
                              if (_options != null) details.onStepContinue!();
                            } else if (details.currentStep == 1) {
                              var success = await _userBloc.execute<bool>(
                                SignUp(
                                  email: _emailController.text,
                                  confirmationCode: _confirmationCodeController.text,
                                  options: _options!,
                                ),
                              );
                              if (success.isTrue) details.onStepContinue!();
                            } else {
                              _userBloc.dispatch(const SaveKeyShareQrCodeImage());
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
                          padding: const EdgeInsets.only(bottom: 12),
                          alignment: Alignment.centerLeft,
                          child: Container(
                            color: Colors.white,
                            width: 273,
                            height: 273,
                            alignment: Alignment.center,
                            child: SizedBox(
                              width: 268,
                              height: 268,
                              child: HtmlElementView(viewType: _iframeManager.iframeKeyShareRender.viewId),
                              // @@NOTE: HtmlElementView expands as much as possible. That's why, if we remove the SizedBox,
                              // it will expand to take whatever size the container allows it, and, therefore, alignment will have
                              // no effect.
                            ),
                          ),
                        ),
                        isActive: true,
                      ),
                    ],
                  ),
                ),
        ),
        // const SizedBox(height: 8),
        // const Divider(color: Colors.white),
        // const SizedBox(height: 8),
        // Column(
        //   children: [
        //     _buildThirdPartyWalletButton('Metamask'),
        //     const SizedBox(height: 6),
        //     _buildThirdPartyWalletButton('CoinbaseWallet'),
        //     const SizedBox(height: 6),
        //     _buildThirdPartyWalletButton('WalletConnect'),
        //   ],
        // ),
      ],
    );
  }
}
