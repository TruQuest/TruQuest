require("@nomiclabs/hardhat-waffle");
require("@nomiclabs/hardhat-etherscan");
require("hardhat-deploy");
require("hardhat-gas-reporter");
require("solidity-coverage");
const { task } = require("hardhat/config");
require("dotenv").config();

module.exports = {
  solidity: "0.8.17",
  defaultNetwork: "ganache",
  networks: {
    localhost: {
      url: "http://localhost:8545",
    },
    ganache: {
      url: "http://localhost:7545",
      chainId: 1337,
      accounts: [
        "0x94303bb1e6b0d634f5253aab6ee321e7c526e337d0c5e3054e30ce948b438eb4",
        "0xe198e7799dc3bdd0d04e778a3a234cc411b9aa71c63835bfc9df40bcfe074a63",
        "0x3afeb4f3a11549842dfcfa98e6aedb18de1d670bd4a55017ce5e1690b1d98966",
        "0x6d1e5f9e96704257cc47d211728f9b3ac399da0efb69e9a5dabd52a05d9c943a",
        "0x2c485a67da1c44263b3cb42782e8cf61d74a14d13ac9c99ec073b2e936750a5b",
        "0x401bd7104cad815c63baf636080d3569ebcd0e308c62ead23d2ae1432f4007f8",
        "0x70fabb03fe5a9b4d429c086f35425653be57170de69c3c9d0a716d3782a79bfd",
        "0x4c56d468a7e3ce180be79c1fdd750d433cbcc148203cad9e374408338fee79bd",
        "0xda87232edd0da6e49116c69e67720de71bf84a9446a95a78a91ba3e6293fc769",
        "0x4dd9d054544240633e8cd9e510c9c25cd5d2a01ef112bbe42e47471990334c60",
      ],
    },
  },
  namedAccounts: {
    deployer: {
      default: 0,
    },
    player: {
      default: 1,
    },
    lotteryPlayer1: {
      default: 2,
    },
    lotteryPlayer2: {
      default: 3,
    },
    lotteryPlayer3: {
      default: 4,
    },
    lotteryPlayer4: {
      default: 5,
    },
  },
};

task("balances", "Player balances").setAction(
  async (_, { getNamedAccounts, ethers }) => {
    const {
      player,
      lotteryPlayer1,
      lotteryPlayer2,
      lotteryPlayer3,
      lotteryPlayer4,
    } = await getNamedAccounts();
    const truQuest = await ethers.getContract("TruQuest");

    let balance = await truQuest.getAvailableFunds(player);
    console.log(
      `Player balance is ${ethers.utils.formatUnits(balance, "wei")}`
    );
    balance = await truQuest.getAvailableFunds(lotteryPlayer1);
    console.log(
      `LotteryPlayer1 balance is ${ethers.utils.formatUnits(balance, "wei")}`
    );
    balance = await truQuest.getAvailableFunds(lotteryPlayer2);
    console.log(
      `LotteryPlayer2 balance is ${ethers.utils.formatUnits(balance, "wei")}`
    );
    balance = await truQuest.getAvailableFunds(lotteryPlayer3);
    console.log(
      `LotteryPlayer3 balance is ${ethers.utils.formatUnits(balance, "wei")}`
    );
    balance = await truQuest.getAvailableFunds(lotteryPlayer4);
    console.log(
      `LotteryPlayer4 balance is ${ethers.utils.formatUnits(balance, "wei")}`
    );
  }
);

task("mine", "Move blocks")
  .addParam("blocks", "Num blocks to mine")
  .setAction(async (taskArgs, hre) => {
    const networkName = hre.network.name;
    if (
      (networkName == "localhost" || networkName == "ganache") &&
      taskArgs.blocks > 0
    ) {
      for (let i = 0; i < taskArgs.blocks; ++i) {
        await hre.network.provider.request({
          method: "evm_mine",
          params: [],
        });
      }
      console.log(`Mined ${taskArgs.blocks} blocks`);
    }
  });

task("lottery", "Enter lottery")
  .addParam("thingId", "Thing GUID")
  .setAction(async (taskArgs, { getNamedAccounts, ethers, network }) => {
    const {
      player,
      lotteryPlayer1,
      lotteryPlayer2,
      lotteryPlayer3,
      lotteryPlayer4,
    } = await getNamedAccounts();

    let verifierLottery = await ethers.getContractAt(
      "VerifierLottery",
      "0xe7f97C0ccE6C7235e89E9875DB4E7B47839aFCd4",
      lotteryPlayer1
    );
    let data = ethers.utils.randomBytes(32);
    let dataHash = await verifierLottery.computeHash(data);
    await verifierLottery.preJoinLottery(taskArgs.thingId, dataHash);
    await network.provider.request({
      method: "evm_mine",
      params: [],
    });
    await verifierLottery.joinLottery(taskArgs.thingId, data);

    verifierLottery = await ethers.getContractAt(
      "VerifierLottery",
      "0xe7f97C0ccE6C7235e89E9875DB4E7B47839aFCd4",
      lotteryPlayer2
    );
    data = ethers.utils.randomBytes(32);
    dataHash = await verifierLottery.computeHash(data);
    await verifierLottery.preJoinLottery(taskArgs.thingId, dataHash);
    await network.provider.request({
      method: "evm_mine",
      params: [],
    });
    await verifierLottery.joinLottery(taskArgs.thingId, data);

    verifierLottery = await ethers.getContractAt(
      "VerifierLottery",
      "0xe7f97C0ccE6C7235e89E9875DB4E7B47839aFCd4",
      lotteryPlayer3
    );
    data = ethers.utils.randomBytes(32);
    dataHash = await verifierLottery.computeHash(data);
    await verifierLottery.preJoinLottery(taskArgs.thingId, dataHash);
    await network.provider.request({
      method: "evm_mine",
      params: [],
    });
    await verifierLottery.joinLottery(taskArgs.thingId, data);

    verifierLottery = await ethers.getContractAt(
      "VerifierLottery",
      "0xe7f97C0ccE6C7235e89E9875DB4E7B47839aFCd4",
      lotteryPlayer4
    );
    data = ethers.utils.randomBytes(32);
    dataHash = await verifierLottery.computeHash(data);
    await verifierLottery.preJoinLottery(taskArgs.thingId, dataHash);
    await network.provider.request({
      method: "evm_mine",
      params: [],
    });
    await verifierLottery.joinLottery(taskArgs.thingId, data);

    verifierLottery = await ethers.getContractAt(
      "VerifierLottery",
      "0xe7f97C0ccE6C7235e89E9875DB4E7B47839aFCd4",
      player
    );
    data = ethers.utils.randomBytes(32);
    dataHash = await verifierLottery.computeHash(data);
    await verifierLottery.preJoinLottery(taskArgs.thingId, dataHash);
    await network.provider.request({
      method: "evm_mine",
      params: [],
    });
    await verifierLottery.joinLottery(taskArgs.thingId, data);

    await network.provider.request({
      method: "evm_mine",
      params: [],
    });
  });
