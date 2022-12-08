const { ethers } = require("hardhat");

module.exports = async ({ getNamedAccounts, deployments, getChainId }) => {
  const { deploy, log } = deployments;
  const {
    deployer,
    player,
    lotteryPlayer1,
    lotteryPlayer2,
    lotteryPlayer3,
    lotteryPlayer4,
  } = await getNamedAccounts();
  const chainId = await getChainId();

  let truthserum = await ethers.getContract("Truthserum", player);

  await deploy("TruQuest", {
    from: deployer,
    args: [truthserum.address, 5, 5, 25, 3, 1, 30, 30],
    log: true,
    waitConfirmations: 1,
  });

  let truQuest = await ethers.getContract("TruQuest", player);
  log(`VerifierLottery deployed at ${await truQuest.s_verifierLottery()}`);
  log(`AcceptancePoll deployed at ${await truQuest.s_acceptancePoll()}`);

  let txnResponse = await truthserum.approve(truQuest.address, 500);
  await txnResponse.wait(1);
  txnResponse = await truQuest.deposit(500);
  await txnResponse.wait(1);

  truthserum = await ethers.getContract("Truthserum", lotteryPlayer1);
  truQuest = await ethers.getContract("TruQuest", lotteryPlayer1);
  txnResponse = await truthserum.approve(truQuest.address, 500);
  await txnResponse.wait(1);
  txnResponse = await truQuest.deposit(500);
  await txnResponse.wait(1);

  truthserum = await ethers.getContract("Truthserum", lotteryPlayer2);
  truQuest = await ethers.getContract("TruQuest", lotteryPlayer2);
  txnResponse = await truthserum.approve(truQuest.address, 500);
  await txnResponse.wait(1);
  txnResponse = await truQuest.deposit(500);
  await txnResponse.wait(1);

  truthserum = await ethers.getContract("Truthserum", lotteryPlayer3);
  truQuest = await ethers.getContract("TruQuest", lotteryPlayer3);
  txnResponse = await truthserum.approve(truQuest.address, 500);
  await txnResponse.wait(1);
  txnResponse = await truQuest.deposit(500);
  await txnResponse.wait(1);

  truthserum = await ethers.getContract("Truthserum", lotteryPlayer4);
  truQuest = await ethers.getContract("TruQuest", lotteryPlayer4);
  txnResponse = await truthserum.approve(truQuest.address, 500);
  await txnResponse.wait(1);
  txnResponse = await truQuest.deposit(500);
  await txnResponse.wait(1);

  let balance = await truQuest.getAvailableFunds(player);
  log(`Player balance is ${ethers.utils.formatUnits(balance, "wei")}`);
  balance = await truQuest.getAvailableFunds(lotteryPlayer1);
  log(`LotteryPlayer1 balance is ${ethers.utils.formatUnits(balance, "wei")}`);
  balance = await truQuest.getAvailableFunds(lotteryPlayer2);
  log(`LotteryPlayer2 balance is ${ethers.utils.formatUnits(balance, "wei")}`);
  balance = await truQuest.getAvailableFunds(lotteryPlayer3);
  log(`LotteryPlayer3 balance is ${ethers.utils.formatUnits(balance, "wei")}`);
  balance = await truQuest.getAvailableFunds(lotteryPlayer4);
  log(`LotteryPlayer4 balance is ${ethers.utils.formatUnits(balance, "wei")}`);
};

module.exports.tags = ["TruQuest"];
