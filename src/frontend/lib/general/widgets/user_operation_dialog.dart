import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../ethereum/models/im/user_operation.dart';
import '../../ethereum/models/vm/user_operation_vm.dart';

// ignore: must_be_immutable
class UserOperationDialog extends StatelessWidget {
  final Stream<UserOperationVm> stream;

  UserOperation? _userOp;

  UserOperationDialog({super.key, required this.stream});

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      backgroundColor: const Color(0xFF242423),
      title: Text(
        'User Operation',
        style: GoogleFonts.philosopher(
          color: Colors.white,
          fontSize: 26,
        ),
      ),
      content: SizedBox(
        width: 500,
        height: 400,
        child: StreamBuilder(
          stream: stream,
          builder: (context, snapshot) {
            if (snapshot.data == null) {
              return const Center(
                child: CircularProgressIndicator(color: Colors.white),
              );
            }

            var userOp = snapshot.data!;
            _userOp = userOp.userOp;

            return Column(
              children: [
                Card(
                  color: Colors.white,
                  shadowColor: Colors.white,
                  elevation: 5,
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(12, 16, 12, 16),
                    child: Column(
                      children: [
                        Row(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Expanded(
                              child: Align(
                                alignment: Alignment.topRight,
                                child: Text(
                                  'From:',
                                  style: GoogleFonts.philosopher(
                                    color: Colors.black,
                                  ),
                                ),
                              ),
                            ),
                            SizedBox(width: 24),
                            Expanded(
                              flex: 3,
                              child: Align(
                                alignment: Alignment.topLeft,
                                child: Tooltip(
                                  message: userOp.sender,
                                  child: Text(
                                    userOp.senderShort,
                                    style: GoogleFonts.raleway(
                                      color: Colors.black,
                                    ),
                                  ),
                                ),
                              ),
                            ),
                          ],
                        ),
                        SizedBox(height: 12),
                        Row(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Expanded(
                              child: Align(
                                alignment: Alignment.topRight,
                                child: Text(
                                  'Operation:',
                                  style: GoogleFonts.philosopher(
                                    color: Colors.black,
                                  ),
                                ),
                              ),
                            ),
                            SizedBox(width: 24),
                            Expanded(
                              flex: 3,
                              child: Align(
                                alignment: Alignment.topLeft,
                                child: Text(
                                  userOp.functionSignature,
                                  style: GoogleFonts.raleway(
                                    color: Colors.black,
                                  ),
                                ),
                              ),
                            ),
                          ],
                        ),
                        if (userOp.hasStake)
                          Padding(
                            padding: const EdgeInsets.only(top: 12),
                            child: Row(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Expanded(
                                  child: Align(
                                    alignment: Alignment.topRight,
                                    child: Text(
                                      'Stake:',
                                      style: GoogleFonts.philosopher(
                                        color: Colors.black,
                                      ),
                                    ),
                                  ),
                                ),
                                SizedBox(width: 24),
                                Expanded(
                                  flex: 3,
                                  child: Align(
                                    alignment: Alignment.topLeft,
                                    child: Tooltip(
                                      message: userOp.stakeSize,
                                      child: Text(
                                        userOp.stakeSizeShort,
                                        style: GoogleFonts.righteous(
                                          color: Colors.black,
                                        ),
                                      ),
                                    ),
                                  ),
                                ),
                              ],
                            ),
                          ),
                        SizedBox(height: 12),
                        Row(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Expanded(
                              child: Align(
                                alignment: Alignment.topRight,
                                child: Text(
                                  'Description:',
                                  style: GoogleFonts.philosopher(
                                    color: Colors.black,
                                  ),
                                ),
                              ),
                            ),
                            SizedBox(width: 24),
                            Expanded(
                              flex: 3,
                              child: Align(
                                alignment: Alignment.topLeft,
                                child: Text(
                                  userOp.description,
                                  style: GoogleFonts.raleway(
                                    color: Colors.black,
                                    fontStyle: FontStyle.italic,
                                  ),
                                ),
                              ),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
                SizedBox(height: 20),
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 12),
                  child: Column(
                    children: [
                      Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Expanded(
                            child: Align(
                              alignment: Alignment.topRight,
                              child: Text(
                                'Estimated gas:',
                                style: GoogleFonts.philosopher(
                                  color: Colors.white,
                                ),
                              ),
                            ),
                          ),
                          SizedBox(width: 24),
                          Expanded(
                            child: Align(
                              alignment: Alignment.topLeft,
                              child: Text(
                                userOp.estimatedGas,
                                style: GoogleFonts.righteous(
                                  color: Colors.white,
                                ),
                              ),
                            ),
                          ),
                        ],
                      ),
                      SizedBox(height: 12),
                      Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Expanded(
                            child: Align(
                              alignment: Alignment.topRight,
                              child: Text(
                                'Estimated transaction fee:',
                                style: GoogleFonts.philosopher(
                                  color: Colors.white,
                                ),
                              ),
                            ),
                          ),
                          SizedBox(width: 24),
                          Expanded(
                            child: Align(
                              alignment: Alignment.topLeft,
                              child: Tooltip(
                                message: userOp.estimatedTxnFee,
                                child: Text(
                                  userOp.estimatedTxnFeeShort,
                                  style: GoogleFonts.righteous(
                                    color: Colors.white,
                                  ),
                                ),
                              ),
                            ),
                          ),
                        ],
                      ),
                      SizedBox(height: 12),
                      Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Expanded(
                            child: Align(
                              alignment: Alignment.topRight,
                              child: Text(
                                'Transaction fee will be paid by the platform:',
                                textAlign: TextAlign.end,
                                style: GoogleFonts.philosopher(
                                  color: Colors.white,
                                ),
                              ),
                            ),
                          ),
                          SizedBox(width: 24),
                          Expanded(
                            child: Align(
                              alignment: Alignment.topLeft,
                              child: Text(
                                userOp.txnFeeCoveredByPaymaster ? 'YES' : 'NO',
                                style: GoogleFonts.righteous(
                                  color: Colors.white,
                                ),
                              ),
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
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
