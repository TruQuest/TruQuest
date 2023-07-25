import 'decision_im.dart';

class NewAssessmentPollVoteIm {
  final String thingId;
  final String proposalId;
  final String castedAt;
  final DecisionIm decision;
  final String reason;

  NewAssessmentPollVoteIm({
    required this.thingId,
    required this.proposalId,
    required this.castedAt,
    required this.decision,
    required this.reason,
  });

  Map<String, dynamic> toJson() => {
        'thingId': thingId,
        'castedAt': castedAt,
        'decision': decision.index,
        'reason': reason,
      };

  String toMessageForSigning() => 'Promise Id: $thingId\n'
      'Settlement Proposal Id: $proposalId\n'
      'Casted At: $castedAt\n'
      'Decision: ${decision.getString()}\n'
      'Reason: ${reason.isNotEmpty ? reason : '(Not Specified)'}';
}
