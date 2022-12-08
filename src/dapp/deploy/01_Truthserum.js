const { ethers } = require("hardhat");

module.exports = async ({ getNamedAccounts, deployments, getChainId }) => {
  const { deploy } = deployments;
  const {
    deployer,
    player,
    lotteryPlayer1,
    lotteryPlayer2,
    lotteryPlayer3,
    lotteryPlayer4,
  } = await getNamedAccounts();
  const chainId = await getChainId();

  await deploy("Truthserum", {
    from: deployer,
    log: true,
    waitConfirmations: 1,
  });

  const truthserum = await ethers.getContract("Truthserum", deployer);
  let txnResponse = await truthserum.transfer(player, 500);
  await txnResponse.wait(1);
  txnResponse = await truthserum.transfer(lotteryPlayer1, 500);
  await txnResponse.wait(1);
  txnResponse = await truthserum.transfer(lotteryPlayer2, 500);
  await txnResponse.wait(1);
  txnResponse = await truthserum.transfer(lotteryPlayer3, 500);
  await txnResponse.wait(1);
  txnResponse = await truthserum.transfer(lotteryPlayer4, 500);
  await txnResponse.wait(1);

  /*
  //If you want to link a library into your contract:
  // reference: https://github.com/austintgriffith/scaffold-eth/blob/using-libraries-example/packages/hardhat/scripts/deploy.js#L19
  const yourContract = await deploy("YourContract", [], {}, {
    LibraryName: **LibraryAddress**
  });
  */
};

module.exports.tags = ["Truthserum"];
