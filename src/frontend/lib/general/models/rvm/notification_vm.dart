import 'watched_item_type_vm.dart';

class NotificationVm {
  final DateTime updateTimestamp;
  final WatchedItemTypeVm itemType;
  final String itemId;
  final int itemUpdateCategory;
  final String title;
  final String? details;

  NotificationVm({
    required this.updateTimestamp,
    required this.itemType,
    required this.itemId,
    required this.itemUpdateCategory,
    required this.title,
    required this.details,
  });

  NotificationVm.fromMap(Map<String, dynamic> map)
      : itemType = WatchedItemTypeVm.values[map['itemType']],
        itemId = map['itemId'],
        itemUpdateCategory = map['itemUpdateCategory'],
        updateTimestamp = DateTime.fromMillisecondsSinceEpoch(
          map['updateTimestamp'],
        ),
        title = map['title'],
        details = map['details'];
}
