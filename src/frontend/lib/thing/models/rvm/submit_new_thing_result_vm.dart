class SubmitNewThingResultVm {
  final String thingId;
  final String signature;

  SubmitNewThingResultVm.fromMap(Map<String, dynamic> map)
      : thingId = map['thingId'],
        signature = map['signature'];
}
