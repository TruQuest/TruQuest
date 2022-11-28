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

const evidenceTD = [{ name: "url", type: "string" }];

const newThingTD = [
  { name: "subjectId", type: "string" },
  { name: "title", type: "string" },
  { name: "details", type: "string" },
  { name: "imageUrl", type: "string" },
  { name: "evidence", type: "EvidenceTD[]" },
  { name: "tags", type: "TagTD[]" },
];

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
  const [subjectId, setSubjectId] = useState("");

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
      tags: [{ id: 1 }],
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

    const response = await axios.post(
      "http://localhost:5223/subject/add",
      {
        input: message,
        signature: res,
      },
      {
        headers: { Authorization: "Bearer " + token },
      }
    );

    const subjectId = response.data.data;
    console.log(`SubjectId: ${subjectId}`);
    if (subjectId) {
      setSubjectId(subjectId);
    }
  };

  const submitNewThing = async () => {
    const message = {
      subjectId: subjectId,
      title: "Moon base",
      details: "2024",
      imageUrl:
        "https://images.newscientist.com/wp-content/uploads/2022/09/09152048/SEI_124263525.jpg",
      evidence: [
        { url: "https://stackoverflow.com/" },
        { url: "https://fanfics.me/" },
      ],
      tags: [{ id: 1 }],
    };

    const data = JSON.stringify({
      types: {
        EIP712Domain: domain,
        NewThingTD: newThingTD,
        EvidenceTD: evidenceTD,
        TagTD: tagTD,
      },
      domain: domainData,
      primaryType: "NewThingTD",
      message: message,
    });

    const res = await web3.provider.request({
      method: "eth_signTypedData_v4",
      params: [account, data],
      from: account,
    });

    const response = await axios.post(
      "http://localhost:5223/thing/submit",
      {
        input: message,
        signature: res,
      },
      {
        headers: { Authorization: "Bearer " + token },
      }
    );
    if (response.data.data) {
      console.log(`ThingId: ${response.data.data.thing.id}`);
      console.log(`Sig: ${response.data.data.signature}`);
    }
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
      {!token && <button onClick={signUp}>Sign up</button>}
      {token && <button onClick={addNewSubject}>Add new subject</button>}
      {token && subjectId && (
        <button onClick={submitNewThing}>Submit new thing</button>
      )}
    </div>
  );
};

export default Home;
