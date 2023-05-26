import 'package:flutter/material.dart';

import '../../user/bloc/user_actions.dart';
import '../../ethereum/bloc/ethereum_bloc.dart';
import '../../ethereum/bloc/ethereum_actions.dart';
import '../../user/bloc/user_bloc.dart';
import '../../user/models/vm/user_vm.dart';
import '../../widget_extensions.dart';

class UserStatusTracker extends StatelessWidgetX {
  late final _userBloc = use<UserBloc>();
  late final _ethereumBloc = use<EthereumBloc>();

  UserStatusTracker({super.key});

  @override
  Widget buildX(BuildContext context) {
    return StreamBuilder(
      stream: _userBloc.currentUser$,
      initialData: _userBloc.latestCurrentUser,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return SizedBox.shrink();
        }

        var user = snapshot.data!.user;
        if (user.state == UserAccountState.guest) {
          return IconButton(
            icon: Icon(
              Icons.wifi_tethering,
              color: Colors.white,
            ),
            onPressed: () => _ethereumBloc.dispatch(
              ConnectEthereumAccount(),
            ),
          );
        } else if (user.state == UserAccountState.connectedNotLoggedIn) {
          return IconButton(
            icon: Icon(
              Icons.door_sliding,
              color: Colors.white,
            ),
            onPressed: () => _userBloc.dispatch(SignInWithEthereum()),
          );
        }

        return Tooltip(
          message: user.ethereumAccount!,
          child: IconButton(
            icon: Icon(
              Icons.account_box,
              color: Colors.white,
            ),
            onPressed: () {},
          ),
        );
      },
    );
  }
}
