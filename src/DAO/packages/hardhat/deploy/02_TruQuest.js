const { ethers } = require("hardhat");

module.exports = async ({ getNamedAccounts, deployments, getChainId }) => {
  const { deploy, log } = deployments;
  const { deployer } = await getNamedAccounts();
  const chainId = await getChainId();

  const truToken = await ethers.getContract("TRU");

  await deploy("TruQuest", {
    from: deployer,
    args: [truToken.address, 10, 5, 25, 3, 1, 10, 10],
    log: true,
    waitConfirmations: 1,
  });

  const truQuest = await ethers.getContract("TruQuest");
  log(`VerifierLottery deployed at ${await truQuest.s_verifierLottery()}`);
  log(`AcceptancePoll deployed at ${await truQuest.s_acceptancePoll()}`);
};

module.exports.tags = ["TruQuest"];
