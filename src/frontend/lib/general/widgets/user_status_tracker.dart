import 'package:flutter/material.dart';

// import '../../user/bloc/user_actions.dart';
// import '../utils/utils.dart';
import 'onboarding_dialog.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';

// ignore: must_be_immutable
class UserStatusTracker extends StatelessWidgetX {
  late final _userBloc = use<UserBloc>();

  UserStatusTracker({super.key});

  @override
  Widget buildX(BuildContext context) {
    return StreamBuilder(
      stream: _userBloc.currentUser$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return const SizedBox.shrink();
        }

        var user = snapshot.data!;

        if (user.id != null) {
          return Tooltip(
            message: user.signerAddress,
            child: IconButton(
              icon: Icon(
                Icons.account_box,
                color: Colors.white,
              ),
              onPressed: () {},
            ),
          );
        } else if (user.originWallet == null) {
          return IconButton(
            icon: Icon(
              Icons.login,
              color: Colors.white,
            ),
            onPressed: () => showDialog(
              context: context,
              builder: (context) => OnboardingDialog(),
            ),
          );
        }

        throw UnsupportedError('Extension wallets are not currently supported');

        // This could only happen with a third party wallet, not embedded one.

        // return IconButton(
        //   icon: Icon(
        //     Icons.login,
        //     color: Colors.white,
        //   ),
        //   onPressed: () => multiStageOffChainFlow(
        //     context,
        //     (ctx) => _userBloc.executeMultiStage(
        //       SignInWithThirdPartyWallet(),
        //       ctx,
        //     ),
        //   ),
        // );
      },
    );
  }
}
