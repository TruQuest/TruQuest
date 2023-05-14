import 'package:shared_preferences/shared_preferences.dart';

class LocalStorage {
  late final SharedPreferences _prefs;

  Future init() async {
    _prefs = await SharedPreferences.getInstance();
  }

  String? getString(String key) => _prefs.getString(key);

  List<String>? getStrings(String key) => _prefs.getStringList(key);

  Future setString(String key, String value) => _prefs.setString(key, value);

  Future setStrings(String key, List<String> value) =>
      _prefs.setStringList(key, value);
}
