class TagIm {
  final int id;

  TagIm({required this.id});

  Map<String, dynamic> toJson() {
    var map = <String, dynamic>{};

    map['id'] = id;

    return map;
  }
}
