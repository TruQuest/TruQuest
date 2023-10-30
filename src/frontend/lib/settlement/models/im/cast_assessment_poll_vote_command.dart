import 'new_settlement_proposal_assessment_poll_vote_im.dart';

class CastAssessmentPollVoteCommand {
  final NewSettlementProposalAssessmentPollVoteIm input;
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
