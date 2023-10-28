CREATE TRIGGER "OnNewJoinedThingSubmissionVerifierLotteryEvent"
BEFORE INSERT ON "JoinedThingSubmissionVerifierLotteryEvents"
FOR EACH ROW
EXECUTE FUNCTION "AddUserIdFromWalletAddress"();

-- CREATE TRIGGER "OnNewCastedAcceptancePollVoteEvent"
-- BEFORE INSERT ON "CastedAcceptancePollVoteEvents"
-- FOR EACH ROW
-- EXECUTE FUNCTION "AddUserIdFromWalletAddress"();

-- CREATE TRIGGER "OnNewJoinedThingAssessmentVerifierLotteryEvent"
-- BEFORE INSERT ON "JoinedThingAssessmentVerifierLotteryEvents"
-- FOR EACH ROW
-- EXECUTE FUNCTION "AddUserIdFromWalletAddress"();

-- CREATE TRIGGER "OnNewThingAssessmentVerifierLotterySpotClaimedEvent"
-- BEFORE INSERT ON "ThingAssessmentVerifierLotterySpotClaimedEvents"
-- FOR EACH ROW
-- EXECUTE FUNCTION "AddUserIdFromWalletAddress"();

-- CREATE TRIGGER "OnNewCastedAssessmentPollVoteEvent"
-- BEFORE INSERT ON "CastedAssessmentPollVoteEvents"
-- FOR EACH ROW
-- EXECUTE FUNCTION "AddUserIdFromWalletAddress"();
