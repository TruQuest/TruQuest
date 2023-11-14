const { BigNumber } = require("ethers");
const { ethers } = require("hardhat");

module.exports = async ({ getNamedAccounts, deployments }) => {
  const { deploy, log } = deployments;
  const { deployer } = await getNamedAccounts();

  let truthserum = await ethers.getContract("Truthserum");

  await deploy("RestrictedAccess", {
    from: deployer,
    log: true,
    waitConfirmations: 1,
  });

  let restrictedAccess = await ethers.getContract("RestrictedAccess");

  await deploy("TruQuest", {
    from: deployer,
    args: [
      truthserum.address,
      5 * 1000000 /* _verifierStake */, // 0.005 TRU
      2 * 1000000 /* _verifierReward */, // 0.002 TRU
      1 * 1000000 /* _verifierPenalty */, // 0.001 TRU
      25 * 1000000 /* _thingStake */, // 0.025 TRU
      7 * 1000000 /* _thingAcceptedReward */, // 0.007 TRU
      3 * 1000000 /* _thingRejectedPenalty */, // 0.003 TRU
      25 * 1000000 /* _settlementProposalStake */, // 0.025 TRU
      7 * 1000000 /* _settlementProposalAcceptedReward */, // 0.007 TRU
      3 * 1000000 /* _settlementProposalRejectedPenalty */, // 0.003 TRU
    ],
    log: true,
    waitConfirmations: 1,
  });

  let truQuest = await ethers.getContract("TruQuest");

  await deploy("ThingValidationVerifierLottery", {
    from: deployer,
    args: [
      truQuest.address,
      3 /* _numVerifiers */,
      70 /* _verifierLotteryDurationBlocks */,
    ],
    log: true,
    waitConfirmations: 1,
  });

  await deploy("ThingValidationPoll", {
    from: deployer,
    args: [
      truQuest.address,
      30 /* _pollDurationBlocks */,
      50 /* _votingVolumeThresholdPercent */,
      51 /* _majorityThresholdPercent */,
    ],
    log: true,
    waitConfirmations: 1,
  });

  await deploy("SettlementProposalAssessmentVerifierLottery", {
    from: deployer,
    args: [
      truQuest.address,
      3 /* _numVerifiers */,
      70 /* _verifierLotteryDurationBlocks */,
    ],
    log: true,
    waitConfirmations: 1,
  });

  await deploy("SettlementProposalAssessmentPoll", {
    from: deployer,
    args: [
      truQuest.address,
      30 /* _pollDurationBlocks */,
      50 /* _votingVolumeThresholdPercent */,
      51 /* _majorityThresholdPercent */,
    ],
    log: true,
    waitConfirmations: 1,
  });

  let tl = await ethers.getContract("ThingValidationVerifierLottery");
  let tp = await ethers.getContract("ThingValidationPoll");
  let spl = await ethers.getContract(
    "SettlementProposalAssessmentVerifierLottery"
  );
  let spp = await ethers.getContract("SettlementProposalAssessmentPoll");

  var txnResponse = await truthserum.setTruQuestAddress(truQuest.address);
  await txnResponse.wait(1);

  txnResponse = await truQuest.setRestrictedAccess(restrictedAccess.address);
  await txnResponse.wait(1);

  txnResponse = await truQuest.setLotteryAndPollAddresses(
    tl.address,
    tp.address,
    spl.address,
    spp.address
  );
  await txnResponse.wait(1);

  txnResponse = await tl.setThingValidationPoll(tp.address);
  await txnResponse.wait(1);

  txnResponse = await tp.setThingValidationVerifierLotteryAddress(tl.address);
  await txnResponse.wait(1);

  txnResponse = await spl.setPolls(tp.address, spp.address);
  await txnResponse.wait(1);

  txnResponse = await spp.setSettlementProposalAssessmentVerifierLotteryAddress(
    spl.address
  );
  await txnResponse.wait(1);
};

module.exports.tags = ["TruQuest"];
