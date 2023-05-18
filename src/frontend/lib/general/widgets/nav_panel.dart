import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:sliver_tools/sliver_tools.dart';

import '../contexts/page_context.dart';
import 'notification_tracker.dart';
import 'user_status_tracker.dart';
import '../../widget_extensions.dart';

class NavPanel extends StatelessWidgetX {
  late final _pageContext = use<PageContext>();

  NavPanel({super.key});

  @override
  Widget buildX(BuildContext context) {
    return SliverPinnedHeader(
      child: Material(
        color: Colors.black,
        elevation: 10,
        child: Container(
          height: 50,
          alignment: Alignment.center,
          child: Row(
            children: [
              NotificationTracker(),
              Spacer(),
              InkWell(
                onTap: () => _pageContext.goto('/subjects'),
                child: Row(
                  children: [
                    Icon(
                      Icons.circle,
                      color: Colors.white,
                      size: 8,
                    ),
                    SizedBox(width: 12),
                    Text(
                      'Subjects',
                      style: GoogleFonts.raleway(
                        fontSize: 17,
                        color: Colors.white,
                      ),
                    ),
                  ],
                ),
              ),
              SizedBox(width: 48),
              InkWell(
                onTap: () => _pageContext.goto('/things'),
                child: Row(
                  children: [
                    Icon(
                      Icons.circle,
                      color: Colors.white,
                      size: 8,
                    ),
                    SizedBox(width: 12),
                    Text(
                      'Things',
                      style: GoogleFonts.raleway(
                        fontSize: 17,
                        color: Colors.white,
                      ),
                    ),
                  ],
                ),
              ),
              SizedBox(width: 48),
              InkWell(
                onTap: () => _pageContext.goto('/how-to'),
                child: Row(
                  children: [
                    Icon(
                      Icons.circle,
                      color: Colors.white,
                      size: 8,
                    ),
                    SizedBox(width: 12),
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
              SizedBox(width: 48),
              InkWell(
                onTap: () => _pageContext.goto('/pong'),
                child: Row(
                  children: [
                    Icon(
                      Icons.circle,
                      color: Colors.white,
                      size: 8,
                    ),
                    SizedBox(width: 12),
                    Text(
                      'Pong!',
                      style: GoogleFonts.raleway(
                        fontSize: 17,
                        color: Colors.white,
                      ),
                    ),
                  ],
                ),
              ),
              SizedBox(width: 48),
              InkWell(
                onTap: () => _pageContext.goto('/goto'),
                child: Row(
                  children: [
                    Icon(
                      Icons.circle,
                      color: Colors.white,
                      size: 8,
                    ),
                    SizedBox(width: 12),
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
              Spacer(),
              Container(
                width: 150,
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
