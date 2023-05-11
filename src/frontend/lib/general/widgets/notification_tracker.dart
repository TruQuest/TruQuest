import 'package:flutter/material.dart';
import 'package:side_sheet/side_sheet.dart';

import '../models/rvm/notification_vm.dart';
import '../services/notifications_cache.dart';
import '../../widget_extensions.dart';

class NotificationTracker extends StatelessWidgetX {
  late final _notificationsCache = use<NotificationsCache>();

  NotificationTracker({super.key});

  Widget _buildNotifications(List<NotificationVm>? notifications) {
    if (notifications == null) {
      return Center(child: CircularProgressIndicator());
    }

    if (notifications.isEmpty) {
      return Center(child: Text('Nothing here yet'));
    }

    return ListView.builder(
      itemCount: notifications.length,
      itemBuilder: (context, index) {
        var notification = notifications[index];
        return ListTile(
          title: Text(notification.title),
          subtitle:
              notification.details != null ? Text(notification.details!) : null,
          onTap: () {},
        );
      },
    );
  }

  @override
  Widget buildX(BuildContext context) {
    return Stack(
      children: [
        StreamBuilder(
          stream: _notificationsCache.unreadNotifications$,
          builder: (context, snapshot) {
            Widget icon;
            if (snapshot.data == null || snapshot.data!.isEmpty) {
              icon = Icon(Icons.notifications);
            } else {
              icon = Icon(
                Icons.notifications_active,
                color: Colors.red,
              );
            }

            return IconButton(
              icon: icon,
              padding: const EdgeInsets.all(12),
              onPressed: () => SideSheet.right(
                context: context,
                width: MediaQuery.of(context).size.width * 0.2,
                body: _buildNotifications(snapshot.data),
              ),
            );
          },
        ),
        Positioned(
          top: 8,
          right: 8,
          child: StreamBuilder(
            stream: _notificationsCache.unreadNotificationsCount$,
            builder: (context, snapshot) {
              var count = snapshot.data ?? 0;
              return count > 0
                  ? Text(
                      count.toString(),
                      style: TextStyle(
                        color: Colors.white,
                      ),
                    )
                  : SizedBox.shrink();
            },
          ),
        ),
      ],
    );
  }
}
