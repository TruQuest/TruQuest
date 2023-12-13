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
    if (documentContext.verdict == null) {
      errors ??= [];
      errors.add('Verdict is not specified');
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
  final int thingVerifiersArrayIndex;

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
    if (thingVerifiersArrayIndex < 0) {
      errors ??= [];
      errors.add('Invalid verifier index');
    }

    return errors;
  }

  const ClaimLotterySpot({
    required this.thingId,
    required this.proposalId,
    required this.thingVerifiersArrayIndex,
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

class GetVotes extends SettlementAction {
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

  const GetVotes({required this.proposalId});
}

class CastVoteOffChain extends SettlementAction {
  final String thingId;
  final String proposalId;
  final DecisionIm decision;
  final String reason;

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
  final int settlementProposalVerifiersArrayIndex;
  final DecisionIm decision;
  final String reason;

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
    if (settlementProposalVerifiersArrayIndex < 0) {
      errors ??= [];
      errors.add('Invalid verifier index');
    }

    return errors;
  }

  const CastVoteOnChain({
    required this.thingId,
    required this.proposalId,
    required this.settlementProposalVerifiersArrayIndex,
    required this.decision,
    required this.reason,
  });
}
