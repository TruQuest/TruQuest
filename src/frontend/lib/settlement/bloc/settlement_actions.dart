import '../../general/utils/utils.dart';
import '../models/im/decision_im.dart';
import '../../general/bloc/actions.dart';
import '../../general/contexts/document_context.dart';

abstract class SettlementAction extends Action {
  const SettlementAction();
}

class CreateNewSettlementProposalDraft extends SettlementAction {
  final DocumentContext documentContext;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (documentContext.thingId == null) {
      errors ??= [];
      errors.add('Promise Id is not set');
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

  const CreateNewSettlementProposalDraft({required this.documentContext});
}

class GetSettlementProposal extends SettlementAction {
  final String proposalId;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!proposalId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Proposal Id');
    }

    return errors;
  }

  const GetSettlementProposal({required this.proposalId});
}

class SubmitNewSettlementProposal extends SettlementAction {
  final String proposalId;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!proposalId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Proposal Id');
    }

    return errors;
  }

  const SubmitNewSettlementProposal({required this.proposalId});
}

class FundSettlementProposal extends SettlementAction {
  final String thingId;
  final String proposalId;
  final String signature;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!thingId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Promise Id');
    }
    if (!proposalId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Proposal Id');
    }
    if (signature.isEmpty) {
      errors ??= [];
      errors.add('Invalid signature');
    }

    return errors;
  }

  const FundSettlementProposal({
    required this.thingId,
    required this.proposalId,
    required this.signature,
  });
}

class GetVerifierLotteryInfo extends SettlementAction {
  final String thingId;
  final String proposalId;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!thingId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Promise Id');
    }
    if (!proposalId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Proposal Id');
    }

    return errors;
  }

  const GetVerifierLotteryInfo({
    required this.thingId,
    required this.proposalId,
  });
}

class GetVerifierLotteryParticipants extends SettlementAction {
  final String thingId;
  final String proposalId;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!thingId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Promise Id');
    }
    if (!proposalId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Proposal Id');
    }

    return errors;
  }

  const GetVerifierLotteryParticipants({
    required this.thingId,
    required this.proposalId,
  });
}

class ClaimLotterySpot extends SettlementAction {
  final String thingId;
  final String proposalId;
  final int userIndexInThingVerifiersArray;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!thingId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Promise Id');
    }
    if (!proposalId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Proposal Id');
    }
    if (userIndexInThingVerifiersArray < 0) {
      errors ??= [];
      errors.add('Invalid index');
    }

    return errors;
  }

  const ClaimLotterySpot({
    required this.thingId,
    required this.proposalId,
    required this.userIndexInThingVerifiersArray,
  });
}

class JoinLottery extends SettlementAction {
  final String thingId;
  final String proposalId;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!thingId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Promise Id');
    }
    if (!proposalId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Proposal Id');
    }

    return errors;
  }

  const JoinLottery({
    required this.thingId,
    required this.proposalId,
  });
}

class GetAssessmentPollInfo extends SettlementAction {
  final String thingId;
  final String proposalId;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!thingId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Promise Id');
    }
    if (!proposalId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Proposal Id');
    }

    return errors;
  }

  const GetAssessmentPollInfo({
    required this.thingId,
    required this.proposalId,
  });
}

class GetVerifiers extends SettlementAction {
  final String proposalId;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!proposalId.isValidUuid) {
      errors ??= [];
      errors.add('Invalid Proposal Id');
    }

    return errors;
  }

  const GetVerifiers({required this.proposalId});
}

class CastVoteOffChain extends SettlementAction {
  final String thingId;
  final String proposalId;
  final DecisionIm decision;
  final String reason;

  const CastVoteOffChain({
    required this.thingId,
    required this.proposalId,
    required this.decision,
    required this.reason,
  });
}

class CastVoteOnChain extends SettlementAction {
  final String thingId;
  final String proposalId;
  final int userIndexInProposalVerifiersArray;
  final DecisionIm decision;
  final String reason;

  const CastVoteOnChain({
    required this.thingId,
    required this.proposalId,
    required this.userIndexInProposalVerifiersArray,
    required this.decision,
    required this.reason,
  });
}
