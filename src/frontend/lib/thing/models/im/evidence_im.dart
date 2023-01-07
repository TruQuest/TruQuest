class EvidenceIm {
  final String url;

  EvidenceIm({required this.url});

  Map<String, dynamic> toJson() {
    var map = <String, dynamic>{};

    map["url"] = url;

    return map;
  }
}
