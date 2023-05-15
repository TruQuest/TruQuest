import '../models/rvm/notification_vm.dart';

abstract class NotificationAction {}

class Dismiss extends NotificationAction {
  final List<NotificationVm> notifications;
  final String? username;

  Dismiss({
    required this.notifications,
    required this.username,
  });
}
