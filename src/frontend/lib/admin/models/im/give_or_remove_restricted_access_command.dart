class GiveOrRemoveRestrictedAccessCommand {
  final List<String> addresses;

  GiveOrRemoveRestrictedAccessCommand({required this.addresses});

  Map<String, dynamic> toJson() => {'addresses': addresses};
}
