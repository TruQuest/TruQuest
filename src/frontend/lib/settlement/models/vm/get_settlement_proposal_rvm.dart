import 'settlement_proposal_vm.dart';

class GetSettlementProposalRvm {
  final SettlementProposalVm proposal;
  final String? signature;

  GetSettlementProposalRvm.fromMap(Map<String, dynamic> map)
      : proposal = SettlementProposalVm.fromMap(map['proposal']),
        signature = map['signature'];

  GetSettlementProposalRvm({
    required this.proposal,
    required this.signature,
  });
}
