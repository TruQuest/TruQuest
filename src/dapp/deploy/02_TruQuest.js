const { ethers } = require("hardhat");

module.exports = async ({ getNamedAccounts, deployments }) => {
  const { deploy, log } = deployments;
  const { deployer } = await getNamedAccounts();

  let truthserum = await ethers.getContract("Truthserum");

  await deploy("TruQuest", {
    from: deployer,
    args: [
      truthserum.address,
      5 /* _verifierStake */,
      2 /* _verifierReward */,
      1 /* _verifierPenalty */,
      25 /* _thingSubmissionStake */,
      7 /* _thingSubmissionAcceptedReward */,
      3 /* _thingSubmissionRejectedPenalty */,
      25 /* _thingSettlementProposalStake */,
      7 /* _thingSettlementProposalAcceptedReward */,
      3 /* _thingSettlementProposalRejectedPenalty */,
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

  var txnResponse = await truQuest.setLotteryAndPollAddresses(
    ts.address,
    acp.address,
    ta.address,
    asp.address
  );
  await txnResponse.wait(1);

  txnResponse = await ts.setAcceptancePoll(acp.address);
  await txnResponse.wait(1);

  txnResponse = await acp.setThingSubmissionVerifierLotteryAddress(ts.address);
  await txnResponse.wait(1);

  txnResponse = await ta.setPolls(acp.address, asp.address);
  await txnResponse.wait(1);

  txnResponse = await asp.setThingAssessmentVerifierLotteryAddress(ta.address);
  await txnResponse.wait(1);
};

module.exports.tags = ["TruQuest"];
