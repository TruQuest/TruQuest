abstract class Action {
  List<String>? validate() => null;

  @override
  String toString() => runtimeType.toString();

  const Action();
}
