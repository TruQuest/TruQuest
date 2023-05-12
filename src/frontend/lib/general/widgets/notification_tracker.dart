import 'package:flutter/material.dart';
import 'package:side_sheet/side_sheet.dart';

import '../bloc/notification_actions.dart';
import '../bloc/notification_bloc.dart';
import '../services/notifications_cache.dart';
import '../../widget_extensions.dart';

class NotificationTracker extends StatelessWidgetX {
  late final _notificationBloc = use<NotificationBloc>();
  late final _notificationsCache = use<NotificationsCache>();

  NotificationTracker({super.key});

  Widget _buildNotificationPanel() {
    return StreamBuilder(
      stream: _notificationsCache.unreadNotifications$,
      builder: (context, snapshot) {
        if (snapshot.data == null || snapshot.data!.isEmpty) {
          return Center(child: Text('Nothing here yet'));
        }

        var notifications = snapshot.data!;

        return Column(
          children: [
            Card(
              margin: const EdgeInsets.fromLTRB(8, 12, 8, 12),
              elevation: 5,
              child: Padding(
                padding: const EdgeInsets.all(6),
                child: Row(
                  children: [
                    Text('${notifications.length} notification(s)'),
                    Spacer(),
                    IconButton(
                      icon: Icon(
                        Icons.delete_sweep,
                        color: Colors.red,
                      ),
                      onPressed: () => _notificationBloc.dispatch(
                        Dismiss(notifications: notifications),
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
                  return ListTile(
                    title: Text(notification.title),
                    subtitle: notification.details != null
                        ? Text(notification.details!)
                        : null,
                    onTap: () {},
                    trailing: IconButton(
                      icon: Icon(
                        Icons.delete,
                        color: Colors.red,
                      ),
                      onPressed: () => _notificationBloc.dispatch(
                        Dismiss(notifications: [notification]),
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
            ? Icon(Icons.notifications)
            : Icon(
                Icons.notifications_active,
                color: Colors.red,
              );

        return Stack(
          children: [
            IconButton(
              icon: icon,
              padding: const EdgeInsets.all(12),
              onPressed: () => SideSheet.right(
                context: context,
                width: MediaQuery.of(context).size.width * 0.2,
                body: _buildNotificationPanel(),
              ),
            ),
            Positioned(
              top: 8,
              right: 8,
              child: count > 0
                  ? Text(
                      count.toString(),
                      style: TextStyle(
                        color: Colors.white,
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
