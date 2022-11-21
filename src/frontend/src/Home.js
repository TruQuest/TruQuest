import { useEffect, useState } from "react";
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

const newSubjectTD = [
  { name: "type", type: "int8" },
  { name: "name", type: "string" },
  { name: "details", type: "string" },
  { name: "imageUrl", type: "string" },
  { name: "tags", type: "TagTD[]" },
];

const tagTD = [{ name: "id", type: "int32" }];

const domainData = {
  name: "TruQuest",
  version: "0.0.1",
  chainId: 31337,
  verifyingContract: "0xCcCCccccCCCCcCCCCCCcCcCccCcCCCcCcccccccC",
  salt: "0xf2d857f4a3edcb9b78b4d503bfe733db1e3f6cdc2b7971ee739626c97e86a558",
};

const Home = () => {
  const { account, enableWeb3, isWeb3EnableLoading, web3 } = useMoralis();
  const [token, setToken] = useState("");

  const connect = async () => {
    await enableWeb3();
  };

  const signUp = async () => {
    const message = {
      username: "Max",
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

    const response = await axios.post("http://localhost:5223/account/signup", {
      input: message,
      signature: res,
    });
    const token = response.data.data.token;
    console.log(token);
    if (token) {
      setToken(token);
    }
  };

  const addNewSubject = async () => {
    const message = {
      type: 0,
      name: "Putin",
      details: "Enemy",
      imageUrl:
        "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b6/Image_created_with_a_mobile_phone.png/640px-Image_created_with_a_mobile_phone.png",
      tags: [{ id: 5 }],
    };

    const data = JSON.stringify({
      types: {
        EIP712Domain: domain,
        NewSubjectTD: newSubjectTD,
        TagTD: tagTD,
      },
      domain: domainData,
      primaryType: "NewSubjectTD",
      message: message,
    });

    const res = await web3.provider.request({
      method: "eth_signTypedData_v4",
      params: [account, data],
      from: account,
    });

    await axios.post(
      "http://localhost:5223/subject/add",
      {
        input: {
          ...message,
          profilePageUrl: "https://www.sports.ru/",
        },
        signature: res,
      },
      {
        headers: { Authorization: "Bearer " + token },
      }
    );
  };

  useEffect(() => {
    if (window.localStorage.getItem("web3account")) {
      enableWeb3();
    }
  }, []);

  // useEffect(() => {
  //   if (account) {
  //     window.localStorage.setItem("web3account", account);
  //   } else {
  //     window.localStorage.removeItem("web3account");
  //   }
  // }, [account]);

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
      {token && <button onClick={addNewSubject}>Add new subject</button>}
    </div>
  );
};

export default Home;
