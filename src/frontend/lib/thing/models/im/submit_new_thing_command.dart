class SubmitNewThingCommand {
  final String thingId;

  SubmitNewThingCommand({required this.thingId});

  Map<String, dynamic> toJson() => {'thingId': thingId};
}
