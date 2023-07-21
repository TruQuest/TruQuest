const { ethers } = require("hardhat");

module.exports = async ({ getNamedAccounts, getChainId }) => {
  const accounts = await getNamedAccounts();
  const chainId = await getChainId();

  if (chainId == 1337 || chainId == 31337) {
    console.log("We are on Ganache!");

    // var faucet = ethers.Wallet.fromMnemonic(
    //   "test test test test test test test test test test test junk"
    // );
    // faucet = faucet.connect(ethers.provider);

    // var bal = await ethers.provider.getBalance(faucet.address);
    // console.log(bal);

    // console.log(faucet);

    // var address = accounts["deployer"];
    // var txnResponse = await faucet.sendTransaction({
    //   to: address,
    //   value: ethers.utils.parseEther("1"),
    // });
    // await txnResponse.wait();

    // var balance = await ethers.provider.getBalance(address);
    // console.log(`${deployer} balance: ${balance}`);

    var deployer = ethers.provider.getSigner(accounts["deployer"]);

    var walletAddresses = [
      "0x69c2ac462AeeD245Fd1A92C789A5d6ccf94b05B7",
      "0xd5938750a90d2B1529bE082dF1030882DEF5dBab",
      "0x334A60c06D394Eef6970A0A6679DDbE767972FeD",
      "0xcaF234cCb63cd528Aeb67Be009230f7a81563E7a",
      "0x81d7125E7EF2ada9171904760D081cc08510C865",
      "0x5d6E95D3b671aC27cacB2E8E61c3EC23f9C226EC",
      "0x6105C4b563E975AF7E814f31b4f900f0129919e9",
      "0x2a171e640EECA4e9DF7985eB8a80a19b3a0b6276",
    ];

    for (var i = 0; i < walletAddresses.length; ++i) {
      var address = walletAddresses[i];
      var txnResponse = await deployer.sendTransaction({
        to: address,
        value: ethers.utils.parseEther("1"),
      });
      await txnResponse.wait();

      var balance = await ethers.provider.getBalance(address);
      console.log(`${address} balance: ${balance}`);
    }
  } else if (chainId == 901) {
    console.log("We are on Optimism Local!");
    var faucet = new ethers.Wallet(
      "ac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80",
      ethers.provider
    );

    var address = accounts["deployer"];
    var txnResponse = await faucet.sendTransaction({
      to: address,
      value: ethers.utils.parseEther("1.0"),
    });
    await txnResponse.wait();

    var balance = await ethers.provider.getBalance(address);
    console.log(`deployer balance: ${balance}`);

    address = accounts["submitter"];
    txnResponse = await faucet.sendTransaction({
      to: address,
      value: ethers.utils.parseEther("1.0"),
    });
    await txnResponse.wait();

    balance = await ethers.provider.getBalance(address);
    console.log(`submitter balance: ${balance}`);

    address = accounts["proposer"];
    txnResponse = await faucet.sendTransaction({
      to: address,
      value: ethers.utils.parseEther("1.0"),
    });
    await txnResponse.wait();

    balance = await ethers.provider.getBalance(address);
    console.log(`proposer balance: ${balance}`);

    for (var i = 1; i <= 10; ++i) {
      var address = accounts["verifier" + i];
      var txnResponse = await faucet.sendTransaction({
        to: address,
        value: ethers.utils.parseEther("1.0"),
      });
      await txnResponse.wait();

      var balance = await ethers.provider.getBalance(address);
      console.log(`verifier${i} balance: ${balance}`);
    }
  }
};
