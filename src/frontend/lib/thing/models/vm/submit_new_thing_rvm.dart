class SubmitNewThingRvm {
  final String thingId;
  final String signature;

  SubmitNewThingRvm.fromMap(Map<String, dynamic> map)
      : thingId = map['thingId'],
        signature = map['signature'];
}
