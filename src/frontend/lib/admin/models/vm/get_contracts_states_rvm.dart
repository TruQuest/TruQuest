import 'subject_vm.dart';
import 'truquest_contract_info_vm.dart';
import 'user_vm.dart';

class GetContractsStatesRvm {
  final TruQuestContractInfoVm truQuestInfo;
  final List<String> whitelistedWalletAddresses;
  final List<UserVm> users;
  final List<SubjectVm> subjects;

  GetContractsStatesRvm.fromMap(Map<String, dynamic> map)
      : truQuestInfo = TruQuestContractInfoVm.fromMap(map['truQuestInfo']),
        whitelistedWalletAddresses = List<String>.unmodifiable(map['whitelistedWalletAddresses'] as List<dynamic>),
        users = List.unmodifiable(
          (map['users'] as List<dynamic>).map((submap) => UserVm.fromMap(submap)),
        ),
        subjects = List.unmodifiable(
          (map['subjects'] as List<dynamic>).map((submap) => SubjectVm.fromMap(submap)),
        );
}
