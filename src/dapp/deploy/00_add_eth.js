const { ethers } = require("hardhat");

module.exports = async ({ getNamedAccounts, getChainId }) => {
  const accounts = await getNamedAccounts();
  const chainId = await getChainId();

  if (chainId == 1337 || chainId == 31337) {
    console.log("We are on Ganache!");

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

    var walletAddresses = [
      "0x48e60BBb664aEfAc9f14aDB42e5FB5b4a119EB66", // entrypoint

      "0xb36b27fD212cC27701899C56E3d65B02E6227fe8",
      "0xD60e165a7F9D7D05aa1EB8cf013D36C1B992353e",
      "0xB8f297bF6d897d1339EC909D9CA3090e6F14187a",
      "0xb745CD594a804Ab2CB18eCdb4888b0C805A771B5",
      "0x8Ef8d7318f13898816a7EA38dAbB3e2A8E733c2C",
      "0x92886220f2A9Cafea6756C18DC9a314226557102",
      "0xCC38c2F9664C2fb88c9B7F7D16A8e77c94c4cf56",
      "0x317176676Af74b9456f2421455CffE048D9A607d",
    ];

    for (var i = 0; i < walletAddresses.length; ++i) {
      var address = walletAddresses[i];
      var txnResponse = await faucet.sendTransaction({
        to: address,
        value: ethers.utils.parseEther("5000"),
      });
      await txnResponse.wait();

      var balance = await ethers.provider.getBalance(address);
      console.log(`${address} balance: ${balance}`);
    }
  }
};
