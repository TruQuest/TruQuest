import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:side_sheet/side_sheet.dart';

import '../contexts/page_context.dart';
import '../bloc/notification_actions.dart';
import '../bloc/notification_bloc.dart';
import '../services/notifications_cache.dart';
import '../../widget_extensions.dart';
import 'clipped_rect.dart';

class NotificationTracker extends StatelessWidgetX {
  late final _notificationBloc = use<NotificationBloc>();
  late final _notificationsCache = use<NotificationsCache>();
  late final _pageContext = use<PageContext>();

  NotificationTracker({super.key});

  Widget _buildNotificationPanel() {
    return StreamBuilder(
      stream: _notificationsCache.unreadNotifications$,
      builder: (context, snapshot) {
        if (snapshot.data == null || snapshot.data!.item1.isEmpty) {
          return Center(
            child: Text(
              'Nothing here yet',
              style: TextStyle(color: Colors.white),
            ),
          );
        }

        var notifications = snapshot.data!.item1;
        var username = snapshot.data!.item2;

        return Column(
          children: [
            Card(
              margin: const EdgeInsets.fromLTRB(8, 12, 8, 12),
              elevation: 5,
              child: Padding(
                padding: const EdgeInsets.all(6),
                child: Row(
                  children: [
                    SizedBox(width: 40),
                    Expanded(
                      child: Text(
                        '${notifications.length} notification(s)',
                        style: GoogleFonts.philosopher(
                          fontSize: 18,
                        ),
                        textAlign: TextAlign.center,
                      ),
                    ),
                    IconButton(
                      icon: Icon(
                        Icons.delete_sweep,
                        color: Colors.red,
                      ),
                      onPressed: () => _notificationBloc.dispatch(
                        Dismiss(
                          notifications: notifications,
                          username: username,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
            Expanded(
              child: ListView.builder(
                itemCount: notifications.length,
                itemBuilder: (context, index) {
                  var notification = notifications[index];
                  return InkWell(
                    onTap: () {
                      _notificationBloc.dispatch(Dismiss(
                        notifications: [notification],
                        username: username,
                      ));

                      _pageContext.goto(notification.itemRoute);
                      Navigator.of(context).pop();
                    },
                    child: Card(
                      margin: const EdgeInsets.fromLTRB(16, 12, 16, 12),
                      elevation: 5,
                      shadowColor: Colors.white,
                      child: Padding(
                        padding: const EdgeInsets.fromLTRB(10, 10, 2, 10),
                        child: Row(
                          children: [
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    notification.title,
                                    style: GoogleFonts.philosopher(
                                      color: Colors.black,
                                      fontSize: 16,
                                    ),
                                  ),
                                  SizedBox(height: 6),
                                  Text(
                                    notification.details!,
                                    style: GoogleFonts.raleway(
                                      color: Colors.black87,
                                    ),
                                    maxLines: 2,
                                  ),
                                ],
                              ),
                            ),
                            IconButton(
                              icon: Icon(
                                Icons.delete,
                                color: Colors.red,
                              ),
                              onPressed: () => _notificationBloc.dispatch(
                                Dismiss(
                                  notifications: [notification],
                                  username: username,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                  );
                },
              ),
            ),
          ],
        );
      },
    );
  }

  @override
  Widget buildX(BuildContext context) {
    return StreamBuilder(
      stream: _notificationsCache.unreadNotificationsCount$,
      builder: (context, snapshot) {
        var count = snapshot.data ?? 0;
        var icon = count == 0
            ? Icon(
                Icons.notifications,
                color: Colors.black,
              )
            : Icon(
                Icons.notifications_active,
                color: Colors.red,
              );

        return Stack(
          children: [
            ClippedRect(
              height: 50,
              color: Colors.white,
              fromNarrowToWide: false,
            ),
            IconButton(
              icon: icon,
              padding: const EdgeInsets.all(12),
              onPressed: () => SideSheet.left(
                context: context,
                sheetColor: Colors.black54,
                width: MediaQuery.of(context).size.width * 0.2,
                body: _buildNotificationPanel(),
              ),
            ),
            Positioned(
              top: 8,
              left: 36,
              child: count > 0
                  ? Text(
                      count.toString(),
                      style: TextStyle(
                        color: Colors.black,
                      ),
                    )
                  : SizedBox.shrink(),
            ),
          ],
        );
      },
    );
  }
}
