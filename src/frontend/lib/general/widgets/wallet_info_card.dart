import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:bot_toast/bot_toast.dart';
import 'package:google_fonts/google_fonts.dart';

import 'deposit_funds_button.dart';
import 'main_page_banner.dart';
import '../../user/bloc/user_bloc.dart';
import 'token_price_tracker.dart';
import '../../widget_extensions.dart';
import 'withdraw_funds_button.dart';

// ignore: must_be_immutable
class WalletInfoCard extends StatelessWidgetX {
  late final _userBloc = use<UserBloc>();

  WalletInfoCard({super.key});

  @override
  Widget buildX(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.end,
      children: [
        TokenPriceTracker(),
        Container(
          color: const Color(0xFF242423),
          height: 284,
          padding: const EdgeInsets.fromLTRB(24, 20, 24, 20),
          child: StreamBuilder(
            stream: _userBloc.smartWalletInfo$,
            builder: (context, snapshot) {
              if (snapshot.data == null) {
                return Center(
                  child: CircularProgressIndicator(color: Colors.white),
                );
              }

              var info = snapshot.data!;

              return Row(
                children: [
                  const MainPageBanner(),
                  const Spacer(),
                  Column(
                    children: [
                      SizedBox(height: 16),
                      Text(
                        'Deposited / Staked',
                        style: GoogleFonts.philosopher(
                          color: Colors.white70,
                          fontSize: 20,
                        ),
                      ),
                      SizedBox(height: 14),
                      info.isPlaceholder
                          ? Text(
                              info.depositedSlashStakedShort,
                              style: GoogleFonts.righteous(
                                color: Colors.white,
                                fontSize: 14,
                              ),
                            )
                          : Tooltip(
                              message: info.depositedSlashStaked,
                              child: Text(
                                info.depositedSlashStakedShort,
                                style: GoogleFonts.righteous(
                                  color: Colors.white,
                                  fontSize: 14,
                                ),
                              ),
                            ),
                      SizedBox(height: 40),
                      DepositFundsButton(),
                      SizedBox(height: 14),
                      WithdrawFundsButton(),
                    ],
                  ),
                  SizedBox(width: 80),
                  Column(
                    children: [
                      SizedBox(height: 16),
                      Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Text(
                            'Wallet',
                            style: GoogleFonts.philosopher(
                              color: Colors.white70,
                              fontSize: 20,
                            ),
                          ),
                          SizedBox(width: 6),
                          Tooltip(
                            message: info.isPlaceholder
                                ? 'Not connected'
                                : info.deployed
                                    ? 'Wallet deployed'
                                    : 'Wallet not yet deployed',
                            child: Icon(
                              Icons.playlist_add_check_circle_outlined,
                              size: 16,
                              color: info.deployed ? Colors.white : Colors.white38,
                            ),
                          ),
                        ],
                      ),
                      SizedBox(height: 8),
                      info.isPlaceholder
                          ? Text(
                              info.addressShort,
                              style: GoogleFonts.raleway(
                                color: Colors.white,
                                fontSize: 21,
                              ),
                            )
                          : Tooltip(
                              message: info.address,
                              child: InkWell(
                                child: Text(
                                  info.addressShort,
                                  style: GoogleFonts.raleway(
                                    color: Colors.white,
                                    fontSize: 21,
                                  ),
                                ),
                                onTap: () async {
                                  await Clipboard.setData(ClipboardData(text: info.address));
                                  BotToast.showText(text: 'Copied');
                                },
                              ),
                            ),
                      Container(
                        width: 120,
                        height: 44,
                        alignment: Alignment(0, -0.35),
                        child: Divider(color: Colors.white60),
                      ),
                      info.isPlaceholder
                          ? Text(
                              info.ethBalanceShort,
                              style: GoogleFonts.righteous(
                                color: Colors.white,
                                fontSize: 14,
                              ),
                            )
                          : Tooltip(
                              message: info.ethBalance,
                              child: Text(
                                info.ethBalanceShort,
                                style: GoogleFonts.righteous(
                                  color: Colors.white,
                                  fontSize: 14,
                                ),
                              ),
                            ),
                      SizedBox(height: 12),
                      info.isPlaceholder
                          ? Text(
                              info.truBalanceShort,
                              style: GoogleFonts.righteous(
                                color: Colors.white,
                                fontSize: 14,
                              ),
                            )
                          : Tooltip(
                              message: info.truBalance,
                              child: Text(
                                info.truBalanceShort,
                                style: GoogleFonts.righteous(
                                  color: Colors.white,
                                  fontSize: 14,
                                ),
                              ),
                            ),
                    ],
                  ),
                  SizedBox(width: 80),
                  Card(
                    color: Colors.white,
                    elevation: 5,
                    shadowColor: Colors.white,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(8),
                    ),
                    clipBehavior: Clip.antiAlias,
                    child: Column(
                      children: [
                        Image.asset(
                          'assets/images/avatar.png',
                          width: 180,
                          height: 200,
                          fit: BoxFit.cover,
                        ),
                        info.isPlaceholder
                            ? Text(
                                info.ownerAddressShort,
                                style: GoogleFonts.raleway(
                                  color: Colors.black,
                                  fontSize: 16,
                                ),
                              )
                            : Tooltip(
                                message: info.ownerAddress,
                                child: InkWell(
                                  child: Text(
                                    info.ownerAddressShort,
                                    style: GoogleFonts.raleway(
                                      color: Colors.black,
                                      fontSize: 16,
                                    ),
                                  ),
                                  onTap: () async {
                                    await Clipboard.setData(ClipboardData(text: info.ownerAddress));
                                    BotToast.showText(text: 'Copied');
                                  },
                                ),
                              ),
                        SizedBox(height: 12),
                      ],
                    ),
                  ),
                ],
              );
            },
          ),
        ),
      ],
    );
  }
}
