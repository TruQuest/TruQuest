import '../../general/utils/utils.dart';
import '../../general/bloc/actions.dart';
import '../../general/contexts/document_context.dart';
import '../models/im/decision_im.dart';

abstract class ThingAction extends Action {
  const ThingAction();
}

class CreateNewThingDraft extends ThingAction {
  final DocumentContext documentContext;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (documentContext.subjectId == null) {
      errors ??= [];
      errors.add('Subject Id is not set');
    }
    if (documentContext.nameOrTitle == null || documentContext.nameOrTitle!.length < 3) {
      errors ??= [];
      errors.add('Title should be at least 3 characters long');
    }
    if (documentContext.operations!.isEmpty) {
      errors ??= [];
      errors.add('Details are not specified');
    }
    if (documentContext.imageExt == null ||
        documentContext.imageBytes == null ||
        documentContext.croppedImageBytes == null) {
      errors ??= [];
      errors.add('No image added');
    }
    if (documentContext.evidence.isEmpty) {
      errors ??= [];
      errors.add('Evidence is not provided');
    }
    if (documentContext.tags.isEmpty) {
      errors ??= [];
      errors.add('Tags are not specified');
    }

    return errors;
  }

  const CreateNewThingDraft({required this.documentContext});
}

class GetThing extends ThingAction {
  final String thingId;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!thingId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Promise Id');
    }

    return errors;
  }

  const GetThing({required this.thingId});
}

class SubmitNewThing extends ThingAction {
  final String thingId;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!thingId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Promise Id');
    }

    return errors;
  }

  const SubmitNewThing({required this.thingId});
}

class FundThing extends ThingAction {
  final String thingId;
  final String signature;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!thingId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Promise Id');
    }
    if (signature.isEmpty) {
      errors ??= [];
      errors.add('Invalid signature');
    }

    return errors;
  }

  const FundThing({required this.thingId, required this.signature});
}

class GetVerifierLotteryInfo extends ThingAction {
  final String thingId;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!thingId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Promise Id');
    }

    return errors;
  }

  const GetVerifierLotteryInfo({required this.thingId});
}

class GetVerifierLotteryParticipants extends ThingAction {
  final String thingId;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!thingId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Promise Id');
    }

    return errors;
  }

  const GetVerifierLotteryParticipants({required this.thingId});
}

class JoinLottery extends ThingAction {
  final String thingId;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!thingId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Promise Id');
    }

    return errors;
  }

  const JoinLottery({required this.thingId});
}

class GetValidationPollInfo extends ThingAction {
  final String thingId;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!thingId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Promise Id');
    }

    return errors;
  }

  const GetValidationPollInfo({required this.thingId});
}

class GetVotes extends ThingAction {
  final String thingId;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!thingId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Promise Id');
    }

    return errors;
  }

  const GetVotes({required this.thingId});
}

class CastVoteOffChain extends ThingAction {
  final String thingId;
  final DecisionIm decision;
  final String reason;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!thingId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Promise Id');
    }

    return errors;
  }

  const CastVoteOffChain({
    required this.thingId,
    required this.decision,
    required this.reason,
  });
}

class CastVoteOnChain extends ThingAction {
  final String thingId;
  final int thingVerifiersArrayIndex;
  final DecisionIm decision;
  final String reason;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!thingId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Promise Id');
    }
    if (thingVerifiersArrayIndex < 0) {
      errors ??= [];
      errors.add('Invalid verifier index');
    }

    return errors;
  }

  const CastVoteOnChain({
    required this.thingId,
    required this.thingVerifiersArrayIndex,
    required this.decision,
    required this.reason,
  });
}

class GetSettlementProposalsList extends ThingAction {
  final String thingId;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!thingId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Promise Id');
    }

    return errors;
  }

  const GetSettlementProposalsList({required this.thingId});
}

class Watch extends ThingAction {
  final String thingId;
  final bool markedAsWatched;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!thingId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Promise Id');
    }

    return errors;
  }

  const Watch({
    required this.thingId,
    required this.markedAsWatched,
  });
}
