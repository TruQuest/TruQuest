CREATE TRIGGER "OnNewJoinedThingValidationVerifierLotteryEvent"
BEFORE INSERT ON "JoinedThingValidationVerifierLotteryEvents"
FOR EACH ROW
EXECUTE FUNCTION "AddUserIdToEvent"();

CREATE TRIGGER "OnNewCastedThingValidationPollVoteEvent"
BEFORE INSERT ON "CastedThingValidationPollVoteEvents"
FOR EACH ROW
EXECUTE FUNCTION "AddUserIdToEvent"();

CREATE TRIGGER "OnNewJoinedSettlementProposalAssessmentVerifierLotteryEvent"
BEFORE INSERT ON "JoinedSettlementProposalAssessmentVerifierLotteryEvents"
FOR EACH ROW
EXECUTE FUNCTION "AddUserIdToEvent"();

CREATE TRIGGER "OnNewClaimedSettlementProposalAssessmentVerifierLotterySpotEvent"
BEFORE INSERT ON "ClaimedSettlementProposalAssessmentVerifierLotterySpotEvents"
FOR EACH ROW
EXECUTE FUNCTION "AddUserIdToEvent"();

CREATE TRIGGER "OnNewCastedSettlementProposalAssessmentPollVoteEvent"
BEFORE INSERT ON "CastedSettlementProposalAssessmentPollVoteEvents"
FOR EACH ROW
EXECUTE FUNCTION "AddUserIdToEvent"();
