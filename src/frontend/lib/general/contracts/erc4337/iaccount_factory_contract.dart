import 'iaccount_contract.dart';

abstract class IAccountFactoryContract {
  IAccountContract get accountContract;
  String get dummySignatureForGasEstimation;
  Future<String> getAddress(String signerAddress);
  String getInitCode(String signerAddress);
}
