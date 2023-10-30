import 'new_thing_validation_poll_vote_im.dart';

class CastValidationPollVoteCommand {
  final NewThingValidationPollVoteIm input;
  final String signature;

  CastValidationPollVoteCommand({
    required this.input,
    required this.signature,
  });

  Map<String, dynamic> toJson() => {
        'input': input.toJson(),
        'signature': signature,
      };
}
