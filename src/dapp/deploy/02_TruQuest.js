const { ethers } = require("hardhat");

module.exports = async ({ getNamedAccounts, deployments, getChainId }) => {
  const { deploy, log } = deployments;
  const { deployer } = await getNamedAccounts();
  const chainId = await getChainId();

  return;

  let truthserum = await ethers.getContract("Truthserum");

  await deploy("TruQuest", {
    from: deployer,
    args: [
      truthserum.address,
      3 /* _numVerifiers */,
      5 /* _verifierStake */,
      25 /* _thingSubmissionStake */,
      7 /* _thingSubmissionAcceptedReward */,
      3 /* _thingSubmissionRejectedPenalty */,
      2 /* _verifierReward */,
      1 /* _verifierPenalty */,
      70 /* _verifierLotteryDurationBlocks */,
      70 /* _pollDurationBlocks */,
      25 /* _thingSettlementProposalStake */,
      // 7 /* _thingSettlementProposalAcceptedReward */,
      // 3 /* _thingSettlementProposalRejectedPenalty */,
      // 50 /* _votingVolumeThresholdPercent */,
      // 51 /* _majorityThresholdPercent */
    ],
    log: true,
    waitConfirmations: 1,
  });

  let truQuest = await ethers.getContract("TruQuest");
  log(
    `ThingSubmissionVerifierLottery deployed at ${await truQuest.s_thingSubmissionVerifierLottery()}`
  );
  log(`AcceptancePoll deployed at ${await truQuest.s_acceptancePoll()}`);
  log(
    `ThingAssessmentVerifierLottery deployed at ${await truQuest.s_thingAssessmentVerifierLottery()}`
  );
  log(`AssessmentPoll deployed at ${await truQuest.s_assessmentPoll()}`);
};

module.exports.tags = ["TruQuest"];
