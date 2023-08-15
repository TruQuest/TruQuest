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
    if (documentContext.details!.isEmpty) {
      errors ??= [];
      errors.add('Details are not specified');
    }
    if (documentContext.evidence.isEmpty) {
      errors ??= [];
      errors.add('Must provide evidence');
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

class GetAcceptancePollInfo extends ThingAction {
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

  const GetAcceptancePollInfo({required this.thingId});
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

  const CastVoteOffChain({
    required this.thingId,
    required this.decision,
    required this.reason,
  });
}

class CastVoteOnChain extends ThingAction {
  final String thingId;
  final int userIndexInThingVerifiersArray;
  final DecisionIm decision;
  final String reason;

  const CastVoteOnChain({
    required this.thingId,
    required this.userIndexInThingVerifiersArray,
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

  const Watch({
    required this.thingId,
    required this.markedAsWatched,
  });
}
