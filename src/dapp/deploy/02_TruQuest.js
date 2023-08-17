const { BigNumber } = require("ethers");
const { ethers } = require("hardhat");

module.exports = async ({ getNamedAccounts, deployments }) => {
  const { deploy, log } = deployments;
  const { deployer } = await getNamedAccounts();

  let truthserum = await ethers.getContract("Truthserum");

  await deploy("TruQuest", {
    from: deployer,
    args: [
      truthserum.address,
      5 * 1000000 /* _verifierStake */, // 0.005 TRU
      2 * 1000000 /* _verifierReward */, // 0.002 TRU
      1 * 1000000 /* _verifierPenalty */, // 0.001 TRU
      25 * 1000000 /* _thingSubmissionStake */, // 0.025 TRU
      7 * 1000000 /* _thingSubmissionAcceptedReward */, // 0.007 TRU
      3 * 1000000 /* _thingSubmissionRejectedPenalty */, // 0.003 TRU
      25 * 1000000 /* _thingSettlementProposalStake */, // 0.025 TRU
      7 * 1000000 /* _thingSettlementProposalAcceptedReward */, // 0.007 TRU
      3 * 1000000 /* _thingSettlementProposalRejectedPenalty */, // 0.003 TRU
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
      35 /* _verifierLotteryDurationBlocks */,
    ],
    log: true,
    waitConfirmations: 1,
  });

  await deploy("AcceptancePoll", {
    from: deployer,
    args: [
      truQuest.address,
      35 /* _pollDurationBlocks */,
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
      35 /* _verifierLotteryDurationBlocks */,
    ],
    log: true,
    waitConfirmations: 1,
  });

  await deploy("AssessmentPoll", {
    from: deployer,
    args: [
      truQuest.address,
      35 /* _pollDurationBlocks */,
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

  var txnResponse = await truthserum.mintTo(
    truQuest.address,
    BigNumber.from("1000000000000000") // 1 000 000 TRU
  );
  await txnResponse.wait(1);

  txnResponse = await truQuest.setLotteryAndPollAddresses(
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
