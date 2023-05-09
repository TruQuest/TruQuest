import 'settlement_proposal_preview_vm.dart';

class GetSettlementProposalsListRvm {
  final String thingId;
  final List<SettlementProposalPreviewVm> proposals;

  GetSettlementProposalsListRvm.fromMap(Map<String, dynamic> map)
      : thingId = map['thingId'],
        proposals = List.unmodifiable(
          (map['proposals'] as List<dynamic>)
              .map((submap) => SettlementProposalPreviewVm.fromMap(submap)),
        );
}
