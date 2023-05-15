import 'watched_item_type_vm.dart';

class NotificationVm {
  final DateTime updateTimestamp;
  final WatchedItemTypeVm itemType;
  final String itemId;
  final int itemUpdateCategory;
  final String title;
  final String? details;

  String get itemRoute =>
      (itemType == WatchedItemTypeVm.subject
          ? '/subjects'
          : itemType == WatchedItemTypeVm.thing
              ? '/things'
              : '/proposals') +
      '/$itemId';

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

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is NotificationVm &&
          runtimeType == other.runtimeType &&
          itemType == other.itemType &&
          itemId == other.itemId &&
          itemUpdateCategory == other.itemUpdateCategory &&
          updateTimestamp == other.updateTimestamp;

  @override
  int get hashCode =>
      '$itemType:$itemId:$itemUpdateCategory:$updateTimestamp'.hashCode;
}
