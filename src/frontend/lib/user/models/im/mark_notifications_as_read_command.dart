import 'notification_im.dart';

class MarkNotificationsAsReadCommand {
  final List<NotificationIm> notifications;

  MarkNotificationsAsReadCommand({required this.notifications});

  Map<String, dynamic> toJson() => {
        'notifications': notifications.map((n) => n.toJson()).toList(),
      };
}
