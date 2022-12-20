const { ethers } = require("hardhat");

module.exports = async ({ getNamedAccounts, deployments, getChainId }) => {
  const { deploy, log } = deployments;
  const { deployer } = await getNamedAccounts();
  const chainId = await getChainId();

  let truthserum = await ethers.getContract("Truthserum");

  await deploy("TruQuest", {
    from: deployer,
    args: [truthserum.address, 5, 5, 25, 3, 1, 100, 100, 25],
    log: true,
    waitConfirmations: 1,
  });

  let truQuest = await ethers.getContract("TruQuest");
  log(`VerifierLottery deployed at ${await truQuest.s_verifierLottery()}`);
  log(`AcceptancePoll deployed at ${await truQuest.s_acceptancePoll()}`);
  log(
    `ThingAssessmentVerifierLottery deployed at ${await truQuest.s_thingAssessmentVerifierLottery()}`
  );
};

module.exports.tags = ["TruQuest"];
