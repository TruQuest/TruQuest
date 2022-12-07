require("dotenv").config();
const { utils } = require("ethers");
const fs = require("fs");
const chalk = require("chalk");
const { task } = require("hardhat/config");

require("@nomicfoundation/hardhat-chai-matchers");
require("@tenderly/hardhat-tenderly");
require("@nomicfoundation/hardhat-toolbox");
require("hardhat-deploy");

const { isAddress, getAddress, formatUnits, parseUnits } = utils;

/*
      📡 This is where you configure your deploy configuration for 🏗 scaffold-eth

      check out `packages/scripts/deploy.js` to customize your deployment

      out of the box it will auto deploy anything in the `contracts` folder and named *.sol
      plus it will use *.args for constructor args
*/

//
// Select the network you want to deploy to here:
//
const defaultNetwork = "ganache";

function mnemonic() {
  try {
    return fs.readFileSync("./mnemonic.txt").toString().trim();
  } catch (e) {
    if (defaultNetwork !== "localhost") {
      console.log(
        "☢️ WARNING: No mnemonic file created for a deploy account. Try `yarn run generate` and then `yarn run account`."
      );
    }
  }
  return "";
}

module.exports = {
  defaultNetwork,
  /**
   * gas reporter configuration that let's you know
   * an estimate of gas for contract deployments and function calls
   * More here: https://hardhat.org/plugins/hardhat-gas-reporter.html
   */
  gasReporter: {
    currency: "USD",
    coinmarketcap: process.env.COINMARKETCAP || null,
    enabled: true,
  },

  // if you want to deploy to a testnet, mainnet, or xdai, you will need to configure:
  // 1. An Infura key (or similar)
  // 2. A private key for the deployer
  // DON'T PUSH THESE HERE!!!
  // An `example.env` has been provided in the Hardhat root. Copy it and rename it `.env`
  // Follow the directions, and uncomment the network you wish to deploy to.

  networks: {
    localhost: {
      url: "http://localhost:8545",
      /*
        notice no mnemonic here? it will just use account 0 of the hardhat node to deploy
        (you can put in a mnemonic here to set the deployer locally)

      */
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
    mainnet: {
      url: "https://mainnet.infura.io/v3/460f40a260564ac4a4f4b3fffb032dad", // <---- YOUR INFURA ID! (or it won't work)
      //      url: "https://speedy-nodes-nyc.moralis.io/XXXXXXXXXXXXXXXXXXXXXXXXX/eth/mainnet", // <---- YOUR MORALIS ID! (not limited to infura)
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    goerli: {
      url: "https://goerli.infura.io/v3/460f40a260564ac4a4f4b3fffb032dad", // <---- YOUR INFURA ID! (or it won't work)
      //      url: "https://speedy-nodes-nyc.moralis.io/XXXXXXXXXXXXXXXXXXXXXXXXX/eth/goerli", // <---- YOUR MORALIS ID! (not limited to infura)
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    xdai: {
      url: "https://rpc.xdaichain.com/",
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    fantom: {
      url: "https://rpcapi.fantom.network",
      gasPrice: 1000000000,
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    testnetFantom: {
      url: "https://rpc.testnet.fantom.network",
      gasPrice: 1000000000,
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    polygon: {
      url: "https://polygon-rpc.com",
      // url: "https://speedy-nodes-nyc.moralis.io/XXXXXXXXXXXXXXXXXXXx/polygon/mainnet", // <---- YOUR MORALIS ID! (not limited to infura)
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    mumbai: {
      url: "https://rpc-mumbai.maticvigil.com",
      // url: "https://speedy-nodes-nyc.moralis.io/XXXXXXXXXXXXXXXXXXXXXXX/polygon/mumbai", // <---- YOUR MORALIS ID! (not limited to infura)
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    matic: {
      url: "https://rpc-mainnet.maticvigil.com/",
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    optimism: {
      url: "https://mainnet.optimism.io",
      accounts: {
        mnemonic: mnemonic(),
      },
      companionNetworks: {
        l1: "mainnet",
      },
    },
    goerliOptimism: {
      url: "https://goerli.optimism.io/",
      accounts: {
        mnemonic: mnemonic(),
      },
      companionNetworks: {
        l1: "goerli",
      },
    },
    localOptimism: {
      url: "http://localhost:8545",
      accounts: {
        mnemonic: mnemonic(),
      },
      companionNetworks: {
        l1: "localOptimismL1",
      },
    },
    localOptimismL1: {
      url: "http://localhost:9545",
      gasPrice: 0,
      accounts: {
        mnemonic: mnemonic(),
      },
      companionNetworks: {
        l2: "localOptimism",
      },
    },
    localAvalanche: {
      url: "http://localhost:9650/ext/bc/C/rpc",
      gasPrice: 225000000000,
      chainId: 43112,
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    fujiAvalanche: {
      url: "https://api.avax-test.network/ext/bc/C/rpc",
      gasPrice: 225000000000,
      chainId: 43113,
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    mainnetAvalanche: {
      url: "https://api.avax.network/ext/bc/C/rpc",
      gasPrice: 225000000000,
      chainId: 43114,
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    testnetHarmony: {
      url: "https://api.s0.b.hmny.io",
      gasPrice: 1000000000,
      chainId: 1666700000,
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    mainnetHarmony: {
      url: "https://api.harmony.one",
      gasPrice: 1000000000,
      chainId: 1666600000,
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    moonbeam: {
      url: "https://rpc.api.moonbeam.network",
      chainId: 1284,
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    moonriver: {
      url: "https://rpc.api.moonriver.moonbeam.network",
      chainId: 1285,
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    moonbaseAlpha: {
      url: "https://rpc.api.moonbase.moonbeam.network",
      chainId: 1287,
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    moonbeamDevNode: {
      url: "http://127.0.0.1:9933",
      chainId: 1281,
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    godwoken: {
      url: "https://godwoken-testnet-v1.ckbapp.dev",
      chainId: 71401,
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    arbitrum: {
      url: "https://arb1.arbitrum.io/rpc",
      chainId: 42161,
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    goerliArbitrum: {
      url: "https://goerli-rollup.arbitrum.io/rpc/",
      chainId: 421613,
      accounts: {
        mnemonic: mnemonic(),
      },
    },
    devnetArbitrum: {
      url: "https://nitro-devnet.arbitrum.io/rpc",
      chainId: 421612,
      accounts: {
        mnemonic: mnemonic(),
      },
    },
  },
  solidity: {
    compilers: [
      {
        version: "0.8.4",
        settings: {
          optimizer: {
            enabled: true,
            runs: 200,
          },
        },
      },
      {
        version: "0.6.7",
        settings: {
          optimizer: {
            enabled: true,
            runs: 200,
          },
        },
      },
    ],
  },
  ovm: {
    solcVersion: "0.7.6",
  },
  namedAccounts: {
    deployer: {
      default: 0, // here this will by default take the first account as deployer
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
  etherscan: {
    apiKey: {
      mainnet: "DNXJA8RX2Q3VZ4URQIWP7Z68CJXQZSC6AW",
      goerli: "DNXJA8RX2Q3VZ4URQIWP7Z68CJXQZSC6AW",
      kovan: "DNXJA8RX2Q3VZ4URQIWP7Z68CJXQZSC6AW",
      rinkeby: "DNXJA8RX2Q3VZ4URQIWP7Z68CJXQZSC6AW",
      ropsten: "DNXJA8RX2Q3VZ4URQIWP7Z68CJXQZSC6AW",
      // add other network's API key here
    },
  },
  abiExporter: {
    path: "../react-app/src/contracts/ABI",
    runOnCompile: true,
    clear: true,
    flat: true,
    only: [],
    spacing: 2,
    pretty: false,
  },
};

const DEBUG = false;

function debug(text) {
  if (DEBUG) {
    console.log(text);
  }
}

task("tru", "Tru")
  .addParam("thingId", "Thing GUID")
  .setAction(async (taskArgs, { getNamedAccounts, ethers }) => {
    const { player } = await getNamedAccounts();
    const acceptancePoll = await ethers.getContractAt(
      "AcceptancePoll",
      "0xBA12646CC07ADBe43F8bD25D83FB628D29C8A762",
      player
    );
    const count = await acceptancePoll.getVerifierCount(taskArgs.thingId);
    console.log(`Verifier count is ${count}`);
  });

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

task("wallet", "Create a wallet (pk) link", async (_, { ethers }) => {
  const randomWallet = ethers.Wallet.createRandom();
  const privateKey = randomWallet._signingKey().privateKey;
  console.log("🔐 WALLET Generated as " + randomWallet.address + "");
  console.log("🔗 http://localhost:3000/pk#" + privateKey);
});

task("fundedwallet", "Create a wallet (pk) link and fund it with deployer?")
  .addOptionalParam(
    "amount",
    "Amount of ETH to send to wallet after generating"
  )
  .addOptionalParam("url", "URL to add pk to")
  .setAction(async (taskArgs, { network, ethers }) => {
    const randomWallet = ethers.Wallet.createRandom();
    const privateKey = randomWallet._signingKey().privateKey;
    console.log("🔐 WALLET Generated as " + randomWallet.address + "");
    const url = taskArgs.url ? taskArgs.url : "http://localhost:3000";

    let localDeployerMnemonic;
    try {
      localDeployerMnemonic = fs.readFileSync("./mnemonic.txt");
      localDeployerMnemonic = localDeployerMnemonic.toString().trim();
    } catch (e) {
      /* do nothing - this file isn't always there */
    }

    const amount = taskArgs.amount ? taskArgs.amount : "0.01";
    const tx = {
      to: randomWallet.address,
      value: ethers.utils.parseEther(amount),
    };

    // SEND USING LOCAL DEPLOYER MNEMONIC IF THERE IS ONE
    // IF NOT SEND USING LOCAL HARDHAT NODE:
    if (localDeployerMnemonic) {
      let deployerWallet = new ethers.Wallet.fromMnemonic(
        localDeployerMnemonic
      );
      deployerWallet = deployerWallet.connect(ethers.provider);
      console.log(
        "💵 Sending " +
          amount +
          " ETH to " +
          randomWallet.address +
          " using deployer account"
      );
      const sendresult = await deployerWallet.sendTransaction(tx);
      console.log("\n" + url + "/pk#" + privateKey + "\n");
    } else {
      console.log(
        "💵 Sending " +
          amount +
          " ETH to " +
          randomWallet.address +
          " using local node"
      );
      console.log("\n" + url + "/pk#" + privateKey + "\n");
      return send(ethers.provider.getSigner(), tx);
    }
  });

task(
  "generate",
  "Create a mnemonic for builder deploys",
  async (_, { ethers }) => {
    const bip39 = require("bip39");
    const hdkey = require("ethereumjs-wallet/hdkey");
    const mnemonic = bip39.generateMnemonic();
    if (DEBUG) console.log("mnemonic", mnemonic);
    const seed = await bip39.mnemonicToSeed(mnemonic);
    if (DEBUG) console.log("seed", seed);
    const hdwallet = hdkey.fromMasterSeed(seed);
    const wallet_hdpath = "m/44'/60'/0'/0/";
    const account_index = 0;
    const fullPath = wallet_hdpath + account_index;
    if (DEBUG) console.log("fullPath", fullPath);
    const wallet = hdwallet.derivePath(fullPath).getWallet();
    const privateKey = "0x" + wallet._privKey.toString("hex");
    if (DEBUG) console.log("privateKey", privateKey);
    const EthUtil = require("ethereumjs-util");
    const address =
      "0x" + EthUtil.privateToAddress(wallet._privKey).toString("hex");
    console.log(
      "🔐 Account Generated as " +
        address +
        " and set as mnemonic in packages/hardhat"
    );
    console.log(
      "💬 Use 'yarn run account' to get more information about the deployment account."
    );

    fs.writeFileSync("./" + address + ".txt", mnemonic.toString());
    fs.writeFileSync("./mnemonic.txt", mnemonic.toString());
  }
);

task(
  "mineContractAddress",
  "Looks for a deployer account that will give leading zeros"
)
  .addParam("searchFor", "String to search for")
  .setAction(async (taskArgs, { network, ethers }) => {
    let contract_address = "";
    let address;

    const bip39 = require("bip39");
    const hdkey = require("ethereumjs-wallet/hdkey");

    let mnemonic = "";
    while (contract_address.indexOf(taskArgs.searchFor) != 0) {
      mnemonic = bip39.generateMnemonic();
      if (DEBUG) console.log("mnemonic", mnemonic);
      const seed = await bip39.mnemonicToSeed(mnemonic);
      if (DEBUG) console.log("seed", seed);
      const hdwallet = hdkey.fromMasterSeed(seed);
      const wallet_hdpath = "m/44'/60'/0'/0/";
      const account_index = 0;
      const fullPath = wallet_hdpath + account_index;
      if (DEBUG) console.log("fullPath", fullPath);
      const wallet = hdwallet.derivePath(fullPath).getWallet();
      const privateKey = "0x" + wallet._privKey.toString("hex");
      if (DEBUG) console.log("privateKey", privateKey);
      const EthUtil = require("ethereumjs-util");
      address =
        "0x" + EthUtil.privateToAddress(wallet._privKey).toString("hex");

      const rlp = require("rlp");
      const keccak = require("keccak");

      const nonce = 0x00; // The nonce must be a hex literal!
      const sender = address;

      const input_arr = [sender, nonce];
      const rlp_encoded = rlp.encode(input_arr);

      const contract_address_long = keccak("keccak256")
        .update(rlp_encoded)
        .digest("hex");

      contract_address = contract_address_long.substring(24); // Trim the first 24 characters.
    }

    console.log(
      "⛏  Account Mined as " +
        address +
        " and set as mnemonic in packages/hardhat"
    );
    console.log(
      "📜 This will create the first contract: " +
        chalk.magenta("0x" + contract_address)
    );
    console.log(
      "💬 Use 'yarn run account' to get more information about the deployment account."
    );

    fs.writeFileSync(
      "./" + address + "_produces" + contract_address + ".txt",
      mnemonic.toString()
    );
    fs.writeFileSync("./mnemonic.txt", mnemonic.toString());
  });

task(
  "account",
  "Get balance informations for the deployment account.",
  async (_, { ethers }) => {
    const hdkey = require("ethereumjs-wallet/hdkey");
    const bip39 = require("bip39");
    try {
      const mnemonic = fs.readFileSync("./mnemonic.txt").toString().trim();
      if (DEBUG) console.log("mnemonic", mnemonic);
      const seed = await bip39.mnemonicToSeed(mnemonic);
      if (DEBUG) console.log("seed", seed);
      const hdwallet = hdkey.fromMasterSeed(seed);
      const wallet_hdpath = "m/44'/60'/0'/0/";
      const account_index = 0;
      const fullPath = wallet_hdpath + account_index;
      if (DEBUG) console.log("fullPath", fullPath);
      const wallet = hdwallet.derivePath(fullPath).getWallet();
      const privateKey = "0x" + wallet._privKey.toString("hex");
      if (DEBUG) console.log("privateKey", privateKey);
      const EthUtil = require("ethereumjs-util");
      const address =
        "0x" + EthUtil.privateToAddress(wallet._privKey).toString("hex");

      const qrcode = require("qrcode-terminal");
      qrcode.generate(address);
      console.log("‍📬 Deployer Account is " + address);
      for (const n in config.networks) {
        // console.log(config.networks[n],n)
        try {
          const provider = new ethers.providers.JsonRpcProvider(
            config.networks[n].url
          );
          const balance = await provider.getBalance(address);
          console.log(" -- " + n + " --  -- -- 📡 ");
          console.log("   balance: " + ethers.utils.formatEther(balance));
          console.log(
            "   nonce: " + (await provider.getTransactionCount(address))
          );
        } catch (e) {
          if (DEBUG) {
            console.log(e);
          }
        }
      }
    } catch (err) {
      console.log(`--- Looks like there is no mnemonic file created yet.`);
      console.log(
        `--- Please run ${chalk.greenBright("yarn generate")} to create one`
      );
    }
  }
);

async function addr(ethers, addr) {
  if (isAddress(addr)) {
    return getAddress(addr);
  }
  const accounts = await ethers.provider.listAccounts();
  if (accounts[addr] !== undefined) {
    return accounts[addr];
  }
  throw `Could not normalize address: ${addr}`;
}

task("accounts", "Prints the list of accounts", async (_, { ethers }) => {
  const accounts = await ethers.provider.listAccounts();
  accounts.forEach((account) => console.log(account));
});

task("blockNumber", "Prints the block number", async (_, { ethers }) => {
  const blockNumber = await ethers.provider.getBlockNumber();
  console.log(blockNumber);
});

task("balance", "Prints an account's balance")
  .addPositionalParam("account", "The account's address")
  .setAction(async (taskArgs, { ethers }) => {
    const balance = await ethers.provider.getBalance(
      await addr(ethers, taskArgs.account)
    );
    console.log(formatUnits(balance, "ether"), "ETH");
  });

function send(signer, txparams) {
  return signer.sendTransaction(txparams, (error, transactionHash) => {
    if (error) {
      debug(`Error: ${error}`);
    }
    debug(`transactionHash: ${transactionHash}`);
    // checkForReceipt(2, params, transactionHash, resolve)
  });
}

task("send", "Send ETH")
  .addParam("from", "From address or account index")
  .addOptionalParam("to", "To address or account index")
  .addOptionalParam("amount", "Amount to send in ether")
  .addOptionalParam("data", "Data included in transaction")
  .addOptionalParam("gasPrice", "Price you are willing to pay in gwei")
  .addOptionalParam("gasLimit", "Limit of how much gas to spend")

  .setAction(async (taskArgs, { network, ethers }) => {
    const from = await addr(ethers, taskArgs.from);
    debug(`Normalized from address: ${from}`);
    const fromSigner = await ethers.provider.getSigner(from);

    let to;
    if (taskArgs.to) {
      to = await addr(ethers, taskArgs.to);
      debug(`Normalized to address: ${to}`);
    }

    const txRequest = {
      from: await fromSigner.getAddress(),
      to,
      value: parseUnits(
        taskArgs.amount ? taskArgs.amount : "0",
        "ether"
      ).toHexString(),
      nonce: await fromSigner.getTransactionCount(),
      gasPrice: parseUnits(
        taskArgs.gasPrice ? taskArgs.gasPrice : "1.001",
        "gwei"
      ).toHexString(),
      gasLimit: taskArgs.gasLimit ? taskArgs.gasLimit : 24000,
      chainId: network.config.chainId,
    };

    if (taskArgs.data !== undefined) {
      txRequest.data = taskArgs.data;
      debug(`Adding data to payload: ${txRequest.data}`);
    }
    debug(txRequest.gasPrice / 1000000000 + " gwei");
    debug(JSON.stringify(txRequest, null, 2));

    return send(fromSigner, txRequest);
  });
