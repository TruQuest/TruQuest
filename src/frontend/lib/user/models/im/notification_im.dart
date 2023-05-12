import 'watched_item_type_im.dart';

class NotificationIm {
  final DateTime updateTimestamp;
  final WatchedItemTypeIm itemType;
  final String itemId;
  final int itemUpdateCategory;

  NotificationIm({
    required this.updateTimestamp,
    required this.itemType,
    required this.itemId,
    required this.itemUpdateCategory,
  });

  Map<String, dynamic> toJson() => {
        'updateTimestamp': updateTimestamp.millisecondsSinceEpoch,
        'itemType': itemType.index,
        'itemId': itemId,
        'itemUpdateCategory': itemUpdateCategory,
      };
}
