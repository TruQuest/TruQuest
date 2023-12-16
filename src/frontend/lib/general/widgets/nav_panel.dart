import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:sliver_tools/sliver_tools.dart';

import 'progress_bar.dart';
import '../contexts/page_context.dart';
import 'notification_tracker.dart';
import 'user_status_tracker.dart';
import '../../widget_extensions.dart';

// ignore: must_be_immutable
class NavPanel extends StatelessWidgetX {
  late final _pageContext = use<PageContext>();

  NavPanel({super.key});

  @override
  Widget buildX(BuildContext context) {
    return SliverPinnedHeader(
      child: Material(
        color: Colors.black,
        elevation: 3,
        child: Container(
          height: 50,
          alignment: Alignment.center,
          child: Row(
            children: [
              const NotificationTracker(),
              SizedBox(
                width: 150,
                child: ProgressBar(),
              ),
              const Spacer(),
              InkWell(
                onTap: () => _pageContext.goto('/subjects'),
                child: Row(
                  children: [
                    const Icon(
                      Icons.circle,
                      color: Colors.white,
                      size: 8,
                    ),
                    const SizedBox(width: 12),
                    Text(
                      'Home',
                      style: GoogleFonts.raleway(
                        fontSize: 17,
                        color: Colors.white,
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 48),
              InkWell(
                onTap: () => _pageContext.goto('/how-to'),
                child: Row(
                  children: [
                    const Icon(
                      Icons.circle,
                      color: Colors.white,
                      size: 8,
                    ),
                    const SizedBox(width: 12),
                    Text(
                      'How To',
                      style: GoogleFonts.raleway(
                        fontSize: 17,
                        color: Colors.white,
                      ),
                    ),
                  ],
                ),
              ),
              if (dotenv.env['ENVIRONMENT'] == 'Development')
                Padding(
                  padding: const EdgeInsets.only(left: 48),
                  child: InkWell(
                    onTap: () => _pageContext.goto('/goto'),
                    child: Row(
                      children: [
                        const Icon(
                          Icons.circle,
                          color: Colors.white,
                          size: 8,
                        ),
                        const SizedBox(width: 12),
                        Text(
                          'Go To',
                          style: GoogleFonts.raleway(
                            fontSize: 17,
                            color: Colors.white,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              const Spacer(),
              Container(
                width: 300,
                alignment: Alignment.centerRight,
                child: UserStatusTracker(),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
