const { ethers } = require("hardhat");

module.exports = async ({ getNamedAccounts, deployments, getChainId }) => {
  const { deploy, log } = deployments;
  const { deployer } = await getNamedAccounts();
  const chainId = await getChainId();

  let truthserum = await ethers.getContract("Truthserum");

  await deploy("TruQuest", {
    from: deployer,
    args: [
      truthserum.address,
      5 /* _numVerifiers */,
      5 /* _verifierStake */,
      25 /* _thingSubmissionStake */,
      7 /* _thingSubmissionAcceptedReward */,
      3 /* _thingSubmissionRejectedPenalty */,
      2 /* _verifierReward */,
      1 /* _verifierPenalty */,
      100 /* _verifierLotteryDurationBlocks */,
      100 /* _pollDurationBlocks */,
      25 /* _thingSettlementProposalStake */,
      // 7 /* _thingSettlementProposalAcceptedReward */,
      // 3 /* _thingSettlementProposalRejectedPenalty */,
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
