import '../models/rvm/notification_vm.dart';
import 'actions.dart';

abstract class NotificationAction extends Action {
  const NotificationAction();
}

class Dismiss extends NotificationAction {
  final List<NotificationVm> notifications;
  final String? userId;

  const Dismiss({
    required this.notifications,
    required this.userId,
  });
}
