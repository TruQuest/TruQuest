const { ethers } = require("hardhat");

module.exports = async ({ getNamedAccounts, deployments, getChainId }) => {
  const { deploy, log } = deployments;
  const { deployer, player } = await getNamedAccounts();
  const chainId = await getChainId();

  const truthserum = await ethers.getContract("Truthserum", player);

  await deploy("TruQuest", {
    from: deployer,
    args: [truthserum.address, 10, 5, 25, 3, 1, 10, 10],
    log: true,
    waitConfirmations: 1,
  });

  const truQuest = await ethers.getContract("TruQuest", player);
  log(`VerifierLottery deployed at ${await truQuest.s_verifierLottery()}`);
  log(`AcceptancePoll deployed at ${await truQuest.s_acceptancePoll()}`);

  let txnResponse = await truthserum.approve(truQuest.address, 500);
  await txnResponse.wait(1);
  txnResponse = await truQuest.deposit(500);
  await txnResponse.wait(1);

  const balance = await truQuest.getAvailableFunds(player);
  log(`Player balance is ${ethers.utils.formatUnits(balance, "wei")}`);
};

module.exports.tags = ["TruQuest"];
