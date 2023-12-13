import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:url_launcher/url_launcher.dart';

class ContactsPanel extends StatelessWidget {
  const ContactsPanel({super.key});

  Widget _buildContact(IconData icon, String contact, String description, {bool tappable = false}) {
    return Row(
      children: [
        Expanded(
          child: Center(
            child: Icon(
              icon,
              color: Colors.blue,
            ),
          ),
        ),
        Expanded(
          flex: 4,
          child: Container(
            decoration: const BoxDecoration(
              border: Border(
                left: BorderSide(color: Colors.white, width: 1),
              ),
            ),
            padding: const EdgeInsets.only(left: 16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  description,
                  style: GoogleFonts.philosopher(color: Colors.white),
                ),
                const SizedBox(height: 6),
                tappable
                    ? InkWell(
                        onTap: () => launchUrl(Uri.parse('https://$contact')),
                        child: SelectableText(
                          contact,
                          style: GoogleFonts.philosopher(
                            color: Colors.white,
                            fontSize: 18,
                          ),
                        ),
                      )
                    : SelectableText(
                        contact,
                        style: GoogleFonts.philosopher(
                          color: Colors.white,
                          fontSize: 18,
                        ),
                      ),
              ],
            ),
          ),
        ),
      ],
    );
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 6),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          _buildContact(
            Icons.dynamic_feed_outlined,
            'twitter.com/tru9quest',
            'You can find my ramblings about development process here:',
            tappable: true,
          ),
          const SizedBox(height: 20),
          _buildContact(
            Icons.email,
            'feedback@truquest.io',
            'Please direct your questions, bug reports, suggestions, etc. here:',
          ),
          const SizedBox(height: 20),
          _buildContact(
            Icons.email,
            'admin@truquest.io',
            'If you want to request access please write here:',
          ),
        ],
      ),
    );
  }
}
