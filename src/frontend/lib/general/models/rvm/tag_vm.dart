class TagVm {
  final int id;
  final String name;

  TagVm.fromMap(Map<String, dynamic> map)
      : id = map['id'],
        name = map['name'];
}
