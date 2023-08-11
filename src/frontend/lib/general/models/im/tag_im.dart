class TagIm {
  final int id;

  TagIm({required this.id});

  @override
  bool operator ==(Object other) => identical(this, other) || other is TagIm && id == other.id;

  @override
  int get hashCode => id.hashCode;
}
