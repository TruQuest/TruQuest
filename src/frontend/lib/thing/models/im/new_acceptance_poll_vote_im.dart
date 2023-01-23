import 'decision_im.dart';

class NewAcceptancePollVoteIm {
  final String castedAt;
  final DecisionIm decision;
  final String reason;

  NewAcceptancePollVoteIm({
    required this.castedAt,
    required this.decision,
    required this.reason,
  });

  Map<String, dynamic> toJson() => {
        'castedAt': castedAt,
        'decision': decision.index,
        'reason': reason,
      };
}
