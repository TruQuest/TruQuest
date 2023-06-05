require("@nomiclabs/hardhat-waffle");
require("@nomiclabs/hardhat-etherscan");
require("hardhat-deploy");
require("hardhat-gas-reporter");
require("solidity-coverage");
const { task } = require("hardhat/config");
require("dotenv").config();

module.exports = {
  solidity: "0.8.17",
  defaultNetwork: "optimismLocal",
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
    optimismLocal: {
      url: "http://localhost:9545",
      chainId: 901,
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
    submitter: {
      default: 1,
    },
    proposer: {
      default: 2,
    },
    verifier1: {
      default: 3,
    },
    verifier2: {
      default: 4,
    },
    verifier3: {
      default: 5,
    },
    verifier4: {
      default: 6,
    },
    verifier5: {
      default: 7,
    },
    verifier6: {
      default: 8,
    },
    verifier7: {
      default: 9,
    },
    verifier8: {
      default: 10,
    },
    verifier9: {
      default: 11,
    },
    verifier10: {
      default: 12,
    },
    verifier11: {
      default: 13,
    },
    verifier12: {
      default: 14,
    },
    verifier13: {
      default: 15,
    },
    verifier14: {
      default: 16,
    },
  },
};
