import '../models/vm/notification_vm.dart';
import 'actions.dart';

abstract class NotificationAction extends Action {
  const NotificationAction();
}

class Dismiss extends NotificationAction {
  final List<NotificationVm> notifications;
  final String? userId;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (notifications.isEmpty) {
      errors ??= [];
      errors.add('No notifications selected');
    }

    return errors;
  }

  const Dismiss({
    required this.notifications,
    required this.userId,
  });
}
