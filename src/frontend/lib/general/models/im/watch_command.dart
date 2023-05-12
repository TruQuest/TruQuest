class WatchCommand {
  final String thingId;
  final bool markedAsWatched;

  WatchCommand({required this.thingId, required this.markedAsWatched});

  Map<String, dynamic> toJson() => {
        'thingId': thingId,
        'markedAsWatched': markedAsWatched,
      };
}
