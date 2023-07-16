const { ethers } = require("hardhat");

module.exports = async ({ getNamedAccounts, getChainId }) => {
  const accounts = await getNamedAccounts();
  const chainId = await getChainId();

  var faucet = ethers.Wallet.fromMnemonic(
    "test test test test test test test test test test test junk"
  );
  faucet = faucet.connect(ethers.provider);

  // var address = accounts["deployer"];
  var address = "0x48469b94EefB39a000617b2FFCd6EA6582f4aFBB";
  console.log(address);
  var txnResponse = await faucet.sendTransaction({
    to: address,
    value: ethers.utils.parseEther("0.1"),
  });
  await txnResponse.wait();

  var balance = await ethers.provider.getBalance(address);
  console.log(`Balance: ${balance}`);

  return;

  if (chainId == 901) {
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

    for (var i = 1; i <= 14; ++i) {
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
