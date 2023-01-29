import 'decision_im.dart';

class NewAssessmentPollVoteIm {
  final String thingId;
  final String castedAt;
  final DecisionIm decision;
  final String reason;

  NewAssessmentPollVoteIm({
    required this.thingId,
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
}
