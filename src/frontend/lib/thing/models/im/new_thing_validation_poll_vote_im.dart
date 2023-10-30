import 'decision_im.dart';

class NewThingValidationPollVoteIm {
  final String thingId;
  final String castedAt;
  final DecisionIm decision;
  final String reason;

  NewThingValidationPollVoteIm({
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

  String toMessageForSigning() => 'Promise Id: $thingId\n'
      'Casted At: $castedAt\n'
      'Decision: ${decision.getString()}\n'
      'Reason: ${reason.isNotEmpty ? reason : '(Not Specified)'}';
}
