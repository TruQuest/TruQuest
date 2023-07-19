const { ethers } = require("hardhat");

module.exports = async ({ getNamedAccounts, deployments }) => {
  const { deploy, log } = deployments;
  const { deployer } = await getNamedAccounts();

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

  await deploy("ThingSubmissionVerifierLottery", {
    from: deployer,
    args: [
      truQuest.address,
      3 /* _numVerifiers */,
      70 /* _verifierLotteryDurationBlocks */,
    ],
    log: true,
    waitConfirmations: 1,
  });

  await deploy("AcceptancePoll", {
    from: deployer,
    args: [
      truQuest.address,
      70 /* _pollDurationBlocks */,
      50 /* _votingVolumeThresholdPercent */,
      51 /* _majorityThresholdPercent */,
    ],
    log: true,
    waitConfirmations: 1,
  });

  await deploy("ThingAssessmentVerifierLottery", {
    from: deployer,
    args: [
      truQuest.address,
      3 /* _numVerifiers */,
      70 /* _verifierLotteryDurationBlocks */,
    ],
    log: true,
    waitConfirmations: 1,
  });

  await deploy("AssessmentPoll", {
    from: deployer,
    args: [
      truQuest.address,
      70 /* _pollDurationBlocks */,
      50 /* _votingVolumeThresholdPercent */,
      51 /* _majorityThresholdPercent */,
    ],
    log: true,
    waitConfirmations: 1,
  });

  let ts = await ethers.getContract("ThingSubmissionVerifierLottery");
  let acp = await ethers.getContract("AcceptancePoll");
  let ta = await ethers.getContract("ThingAssessmentVerifierLottery");
  let asp = await ethers.getContract("AssessmentPoll");

  var txnResponse = await ts.connectToAcceptancePoll(acp.address);
  await txnResponse.wait(1);

  txnResponse = await acp.connectToThingSubmissionVerifierLottery(ts.address);
  await txnResponse.wait(1);

  txnResponse = await ta.connectToAcceptancePoll(acp.address);
  await txnResponse.wait(1);

  txnResponse = await ta.connectToAssessmentPoll(asp.address);
  await txnResponse.wait(1);

  txnResponse = await asp.connectToThingAssessmentVerifierLottery(ta.address);
  await txnResponse.wait(1);

  // log(
  //   `ThingSubmissionVerifierLottery deployed at ${await truQuest.s_thingSubmissionVerifierLottery()}`
  // );
  // log(`AcceptancePoll deployed at ${await truQuest.s_acceptancePoll()}`);
  // log(
  //   `ThingAssessmentVerifierLottery deployed at ${await asp.s_verifierLottery()}`
  // );
  // log(`AssessmentPoll deployed at ${await truQuest.s_assessmentPoll()}`);
};

module.exports.tags = ["TruQuest"];
