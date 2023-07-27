import 'package:flutter/material.dart';

import '../../ethereum/models/im/user_operation.dart';

// ignore: must_be_immutable
class UserOperationDialog extends StatelessWidget {
  final Stream<UserOperation> stream;

  UserOperation? _userOp;

  UserOperationDialog({super.key, required this.stream});

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('User Operation'),
      content: SizedBox(
        width: 400,
        height: 600,
        child: StreamBuilder(
          stream: stream,
          builder: (context, snapshot) {
            if (snapshot.data == null) {
              return const Center(
                child: CircularProgressIndicator(),
              );
            }

            var userOp = _userOp = snapshot.data!;
            return Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('From: ${userOp.sender}'),
                Text('Max fee per gas: ${userOp.maxFeePerGas}'),
                Text('Total gas: ${userOp.totalProvisionedGas}'),
              ],
            );
          },
        ),
      ),
      actions: [
        TextButton(
          child: const Text('Ok'),
          onPressed: () => Navigator.of(context).pop(_userOp),
        ),
      ],
    );
  }
}
