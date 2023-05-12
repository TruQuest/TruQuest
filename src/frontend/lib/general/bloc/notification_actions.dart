import '../models/rvm/notification_vm.dart';

abstract class NotificationAction {}

class Dismiss extends NotificationAction {
  final List<NotificationVm> notifications;

  Dismiss({required this.notifications});
}
