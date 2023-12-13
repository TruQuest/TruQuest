import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

class WalletSelectionDialog extends StatelessWidget {
  const WalletSelectionDialog({super.key});

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      backgroundColor: const Color(0xFF242423),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              InkWell(
                onTap: () => Navigator.of(context).pop('Local'),
                child: Card(
                  color: const Color(0xffF8F9FA),
                  shadowColor: Colors.white,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                  elevation: 5,
                  child: Container(
                    width: 216,
                    height: 86,
                    padding: const EdgeInsets.all(8),
                    alignment: Alignment.center,
                    child: Text(
                      'Local',
                      style: GoogleFonts.righteous(
                        color: Colors.black,
                        fontSize: 30,
                      ),
                    ),
                  ),
                ),
              ),
              const SizedBox(width: 20),
              InkWell(
                onTap: () => Navigator.of(context).pop('WalletConnect'),
                child: Card(
                  color: const Color(0xffF8F9FA),
                  shadowColor: Colors.white,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                  elevation: 5,
                  child: Padding(
                    padding: const EdgeInsets.all(8),
                    child: Image.asset(
                      'assets/images/walletconnect.png',
                      width: 200,
                      height: 70,
                      fit: BoxFit.contain,
                    ),
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 20),
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              InkWell(
                onTap: () => Navigator.of(context).pop('Metamask'),
                child: Card(
                  color: const Color(0xffF8F9FA),
                  shadowColor: Colors.white,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                  elevation: 5,
                  child: Padding(
                    padding: const EdgeInsets.all(8),
                    child: Image.asset(
                      'assets/images/metamask.png',
                      width: 200,
                      height: 70,
                      fit: BoxFit.contain,
                    ),
                  ),
                ),
              ),
              const SizedBox(width: 20),
              InkWell(
                onTap: () => Navigator.of(context).pop('CoinbaseWallet'),
                child: Card(
                  color: const Color(0xffF8F9FA),
                  shadowColor: Colors.white,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                  elevation: 5,
                  child: Padding(
                    padding: const EdgeInsets.all(8),
                    child: Image.asset(
                      'assets/images/coinbase.png',
                      width: 200,
                      height: 70,
                      fit: BoxFit.contain,
                    ),
                  ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
