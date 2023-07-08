import '../models/rvm/notification_vm.dart';

abstract class NotificationAction {
  const NotificationAction();
}

class Dismiss extends NotificationAction {
  final List<NotificationVm> notifications;
  final String? username;

  const Dismiss({
    required this.notifications,
    required this.username,
  });
}
