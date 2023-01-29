import 'new_assessment_poll_vote_im.dart';

class CastAssessmentPollVoteCommand {
  final NewAssessmentPollVoteIm input;
  final String signature;

  CastAssessmentPollVoteCommand({
    required this.input,
    required this.signature,
  });

  Map<String, dynamic> toJson() => {
        'input': input.toJson(),
        'signature': signature,
      };
}
