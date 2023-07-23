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

  Map<String, dynamic> toJsonForSigning() => {
        'thingId': thingId,
        'settlementProposalId': proposalId,
        'castedAt': castedAt,
        'decision': decision.getString(),
        'reason': reason,
      };
}
