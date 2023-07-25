abstract class Action {
  bool get mustValidate => false;
  List<String>? validate() => null;

  const Action();
}
