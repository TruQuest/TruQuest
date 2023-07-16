import 'iaccount_contract.dart';

abstract class IAccountFactoryContract {
  IAccountContract get accountContract;
  String get dummySignatureForGasEstimation;
  Future<String> getAddress(String ownerAddress);
  String getInitCode(String ownerAddress);
}
