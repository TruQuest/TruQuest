import 'decision_im.dart';

class NewAcceptancePollVoteIm {
  final String thingId;
  final String castedAt;
  final DecisionIm decision;
  final String reason;

  NewAcceptancePollVoteIm({
    required this.thingId,
    required this.castedAt,
    required this.decision,
    required this.reason,
  });

  Map<String, dynamic> toJson() => {
        'castedAt': castedAt,
        'decision': decision.index,
        'reason': reason,
      };

  Map<String, dynamic> toJsonForSigning() => {
        'thingId': thingId,
        'castedAt': castedAt,
        'decision': decision.getString(),
        'reason': reason,
      };
}
