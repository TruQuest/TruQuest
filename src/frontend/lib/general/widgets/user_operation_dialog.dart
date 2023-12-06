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
            if (snapshot.hasError) {
              _userOp = null;
              return Center(
                child: Text(
                  snapshot.error.toString(),
                  style: GoogleFonts.raleway(
                    color: Colors.white,
                    fontSize: 18,
                  ),
                ),
              );
            } else if (snapshot.data == null) {
              _userOp = null;
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
                              child: Text(
                                'From:',
                                textAlign: TextAlign.end,
                                style: GoogleFonts.philosopher(
                                  color: Colors.black,
                                ),
                              ),
                            ),
                            SizedBox(width: 24),
                            Expanded(
                              flex: 3,
                              child: Tooltip(
                                message: userOp.sender,
                                child: Text(
                                  userOp.senderShort,
                                  textAlign: TextAlign.start,
                                  style: GoogleFonts.raleway(
                                    color: Colors.black,
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
                              child: Text(
                                'Operation:',
                                textAlign: TextAlign.end,
                                style: GoogleFonts.philosopher(
                                  color: Colors.black,
                                ),
                              ),
                            ),
                            SizedBox(width: 24),
                            Expanded(
                              flex: 3,
                              child: Text(
                                userOp.functionSignature,
                                textAlign: TextAlign.start,
                                style: GoogleFonts.raleway(
                                  color: Colors.black,
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
                                  child: Text(
                                    'Truthserum amount:',
                                    textAlign: TextAlign.end,
                                    style: GoogleFonts.philosopher(
                                      color: Colors.black,
                                    ),
                                  ),
                                ),
                                SizedBox(width: 24),
                                Expanded(
                                  flex: 3,
                                  child: Tooltip(
                                    message: userOp.stakeSize,
                                    child: Text(
                                      userOp.stakeSizeShort,
                                      textAlign: TextAlign.start,
                                      style: GoogleFonts.righteous(
                                        color: Colors.black,
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
                              child: Text(
                                'Description:',
                                textAlign: TextAlign.end,
                                style: GoogleFonts.philosopher(
                                  color: Colors.black,
                                ),
                              ),
                            ),
                            SizedBox(width: 24),
                            Expanded(
                              flex: 3,
                              child: Text(
                                userOp.description,
                                textAlign: TextAlign.start,
                                style: GoogleFonts.raleway(
                                  color: Colors.black,
                                  fontStyle: FontStyle.italic,
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
                            child: Text(
                              'Estimated gas:',
                              textAlign: TextAlign.end,
                              style: GoogleFonts.philosopher(
                                color: Colors.white,
                              ),
                            ),
                          ),
                          SizedBox(width: 24),
                          Expanded(
                            child: Text(
                              userOp.estimatedGas,
                              textAlign: TextAlign.start,
                              style: GoogleFonts.righteous(
                                color: Colors.white,
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
                            child: Text(
                              'Estimated transaction fee:',
                              textAlign: TextAlign.end,
                              style: GoogleFonts.philosopher(
                                color: Colors.white,
                              ),
                            ),
                          ),
                          SizedBox(width: 24),
                          Expanded(
                            child: Tooltip(
                              message: userOp.estimatedTxnFee,
                              child: Text(
                                userOp.estimatedTxnFeeShort,
                                textAlign: TextAlign.start,
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
                            child: Text(
                              'Transaction fee will be paid by the platform:',
                              textAlign: TextAlign.end,
                              style: GoogleFonts.philosopher(
                                color: Colors.white,
                              ),
                            ),
                          ),
                          SizedBox(width: 24),
                          Expanded(
                            child: Text(
                              userOp.txnFeeCoveredByPaymaster ? 'YES' : 'NO',
                              textAlign: TextAlign.start,
                              style: GoogleFonts.righteous(
                                color: Colors.white,
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
      actionsPadding: const EdgeInsets.only(right: 12, bottom: 12),
      actions: [
        ElevatedButton(
          style: ElevatedButton.styleFrom(
            backgroundColor: Colors.white,
            foregroundColor: Colors.black,
          ),
          child: Text(
            'Cancel',
            style: GoogleFonts.raleway(
              fontWeight: FontWeight.bold,
            ),
          ),
          onPressed: () => Navigator.of(context).pop(null),
        ),
        SizedBox(width: 6),
        ElevatedButton(
          style: ElevatedButton.styleFrom(
            backgroundColor: Colors.white,
            foregroundColor: Colors.black,
          ),
          child: Text(
            'Confirm',
            style: GoogleFonts.raleway(
              fontWeight: FontWeight.bold,
            ),
          ),
          onPressed: () => Navigator.of(context).pop(_userOp),
        ),
      ],
    );
  }
}
