import { useEffect } from "react";
import { useMoralis } from "react-moralis";
import axios from "axios";

const domain = [
  { name: "name", type: "string" },
  { name: "version", type: "string" },
  { name: "chainId", type: "uint256" },
  { name: "verifyingContract", type: "address" },
  { name: "salt", type: "bytes32" },
];

const signUpTD = [{ name: "username", type: "string" }];

const domainData = {
  name: "TruQuest",
  version: "0.0.1",
  chainId: 31337,
  verifyingContract: "0xCcCCccccCCCCcCCCCCCcCcCccCcCCCcCcccccccC",
  salt: "0xf2d857f4a3edcb9b78b4d503bfe733db1e3f6cdc2b7971ee739626c97e86a558",
};

const Home = () => {
  const { account, enableWeb3, isWeb3EnableLoading, web3 } = useMoralis();

  const connect = async () => {
    await enableWeb3();
  };

  const signUp = async () => {
    const message = {
      username: "Dimitar",
    };

    const data = JSON.stringify({
      types: {
        EIP712Domain: domain,
        SignUpTD: signUpTD,
      },
      domain: domainData,
      primaryType: "SignUpTD",
      message: message,
    });

    const res = await web3.provider.request({
      method: "eth_signTypedData_v4",
      params: [account, data],
      from: account,
    });

    await axios.post("http://localhost:5223/account/signup", {
      input: message,
      signature: res,
    });
  };

  useEffect(() => {
    if (window.localStorage.getItem("web3account")) {
      enableWeb3();
    }
  }, []);

  useEffect(() => {
    if (account) {
      window.localStorage.setItem("web3account", account);
    } else {
      window.localStorage.removeItem("web3account");
    }
  }, [account]);

  if (!account) {
    return (
      <button onClick={connect} disabled={isWeb3EnableLoading}>
        Connect
      </button>
    );
  }

  return (
    <div>
      <h3>{account}</h3>
      <button onClick={signUp}>Sign up</button>
    </div>
  );
};

export default Home;
