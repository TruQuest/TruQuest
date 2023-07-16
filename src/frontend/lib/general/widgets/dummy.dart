import 'package:flutter/material.dart';

import '../../user/services/user_service.dart';
import '../../widget_extensions.dart';
import 'sign_in_stepper.dart';

class Dummy extends StatefulWidget {
  const Dummy({super.key});

  @override
  State<Dummy> createState() => _DummyState();
}

class _DummyState extends StateX<Dummy> {
  late final _userService = use<UserService>();

  @override
  Widget buildX(BuildContext context) {
    var a = _userService;
    return Scaffold(
      body: Center(
        child: IconButton(
          icon: Icon(Icons.add),
          onPressed: () {
            showDialog(
              context: context,
              builder: (context) => SignInStepper(),
            );
          },
        ),
      ),
      floatingActionButton: FloatingActionButton(
        child: Icon(Icons.abc),
        onPressed: () async {
          await _userService.foo();
        },
      ),
    );
  }
}
