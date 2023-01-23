import 'new_acceptance_poll_vote_im.dart';

class CastAcceptancePollVoteCommand {
  final NewAcceptancePollVoteIm input;
  final String signature;

  CastAcceptancePollVoteCommand({
    required this.input,
    required this.signature,
  });

  Map<String, dynamic> toJson() => {
        'input': input.toJson(),
        'signature': signature,
      };
}
