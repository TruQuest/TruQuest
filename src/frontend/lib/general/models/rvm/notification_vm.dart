import 'watched_item_type_vm.dart';

class NotificationVm {
  final DateTime updateTimestamp;
  final WatchedItemTypeVm itemType;
  final String itemId;
  final String title;
  final String? details;

  NotificationVm({
    required this.updateTimestamp,
    required this.itemType,
    required this.itemId,
    required this.title,
    required this.details,
  });
}
