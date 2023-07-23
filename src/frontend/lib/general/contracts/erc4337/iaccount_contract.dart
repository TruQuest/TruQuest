abstract class IAccountContract {
  String execute((String, String) targetAndCallData);
  String executeBatch(List<(String, String)> targetAndCallDataList);
}
