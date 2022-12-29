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
      chainId: 51234,
      accounts: {
        mnemonic:
          "atom traffic guard castle father vendor modify sauce rebuild true mixture van",
      },
    },
  },
  namedAccounts: {
    deployer: {
      default: 0,
    },
  },
};
