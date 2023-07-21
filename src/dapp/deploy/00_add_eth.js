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
      "0x20FD69D46DC690ef926d209FF016398D6613F168",
      "0x29b9B8924cD0c6eae70981f611f3A2a07AC61f16",
      "0xFC2a6bE9D03eb0F4Db06EaBCac63be3f5002A09B",
      "0x0aB37d130deD0a85fCf2d472ac7aef1650C3CaaE",
      "0x881606962701F9483d1D5FAD45d48C27Ec9698E7",
      "0xaB45E127Fd54B2302E0B1c76d0444b50E12D6d1B",
      "0x297c19fb45f0a4022c6D7030f21696207e51B9B8",
      "0x9914DADEe4De641Da1f124Fc6026535be249ECc8",
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
