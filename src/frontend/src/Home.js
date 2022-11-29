import { useEffect, useState } from "react";
import { useMoralis, useWeb3Contract } from "react-moralis";
import axios from "axios";

const domain = [
  { name: "name", type: "string" },
  { name: "version", type: "string" },
  { name: "chainId", type: "uint256" },
  { name: "verifyingContract", type: "address" },
  { name: "salt", type: "bytes32" },
];

const signUpTd = [{ name: "username", type: "string" }];

const newSubjectTd = [
  { name: "type", type: "int8" },
  { name: "name", type: "string" },
  { name: "details", type: "string" },
  { name: "imageUrl", type: "string" },
  { name: "tags", type: "TagTd[]" },
];

const tagTd = [{ name: "id", type: "int32" }];

const evidenceTd = [{ name: "url", type: "string" }];

const newThingTd = [
  { name: "subjectId", type: "string" },
  { name: "title", type: "string" },
  { name: "details", type: "string" },
  { name: "imageUrl", type: "string" },
  { name: "evidence", type: "EvidenceTd[]" },
  { name: "tags", type: "TagTd[]" },
];

const domainData = {
  name: "TruQuest",
  version: "0.0.1",
  chainId: 31337,
  verifyingContract: "0x9fE46736679d2D9a65F0992F2272dE9f3c7fa6e0",
  salt: "0xf2d857f4a3edcb9b78b4d503bfe733db1e3f6cdc2b7971ee739626c97e86a558",
};

const Home = () => {
  const { account, enableWeb3, isWeb3EnableLoading, web3 } = useMoralis();
  const [token, setToken] = useState("");
  const [subjectId, setSubjectId] = useState("");
  const [thingId, setThingId] = useState("");
  const [sig, setSig] = useState("");

  const { runContractFunction } = useWeb3Contract({
    contractAddress: "0x9fE46736679d2D9a65F0992F2272dE9f3c7fa6e0",
    abi: [
      {
        inputs: [
          {
            internalType: "address",
            name: "_truthserumAddress",
            type: "address",
          },
          {
            internalType: "uint8",
            name: "_numVerifiers",
            type: "uint8",
          },
          {
            internalType: "uint256",
            name: "_verifierStake",
            type: "uint256",
          },
          {
            internalType: "uint256",
            name: "_thingStake",
            type: "uint256",
          },
          {
            internalType: "uint256",
            name: "_thingSubmissionAcceptedReward",
            type: "uint256",
          },
          {
            internalType: "uint256",
            name: "_verifierReward",
            type: "uint256",
          },
          {
            internalType: "uint16",
            name: "_verifierLotteryDurationBlocks",
            type: "uint16",
          },
          {
            internalType: "uint16",
            name: "_acceptancePollDurationBlocks",
            type: "uint16",
          },
        ],
        stateMutability: "nonpayable",
        type: "constructor",
      },
      {
        inputs: [],
        name: "TruQuest__InvalidSignature",
        type: "error",
      },
      {
        inputs: [],
        name: "TruQuest__NotAcceptancePoll",
        type: "error",
      },
      {
        inputs: [
          {
            internalType: "uint256",
            name: "requiredAmount",
            type: "uint256",
          },
          {
            internalType: "uint256",
            name: "availableAmount",
            type: "uint256",
          },
        ],
        name: "TruQuest__NotEnoughFunds",
        type: "error",
      },
      {
        inputs: [],
        name: "TruQuest__NotVerifierLottery",
        type: "error",
      },
      {
        inputs: [
          {
            internalType: "string",
            name: "thingId",
            type: "string",
          },
        ],
        name: "TruQuest__ThingAlreadyFunded",
        type: "error",
      },
      {
        anonymous: false,
        inputs: [
          {
            indexed: true,
            internalType: "address",
            name: "user",
            type: "address",
          },
          {
            indexed: false,
            internalType: "uint256",
            name: "amount",
            type: "uint256",
          },
        ],
        name: "FundsDeposited",
        type: "event",
      },
      {
        anonymous: false,
        inputs: [
          {
            indexed: true,
            internalType: "string",
            name: "thingId",
            type: "string",
          },
          {
            indexed: true,
            internalType: "address",
            name: "user",
            type: "address",
          },
          {
            indexed: false,
            internalType: "uint256",
            name: "thingStake",
            type: "uint256",
          },
        ],
        name: "ThingFunded",
        type: "event",
      },
      {
        inputs: [
          {
            internalType: "address",
            name: "_user",
            type: "address",
          },
          {
            internalType: "uint256",
            name: "_requiredFunds",
            type: "uint256",
          },
        ],
        name: "checkHasAtLeast",
        outputs: [
          {
            internalType: "bool",
            name: "",
            type: "bool",
          },
        ],
        stateMutability: "view",
        type: "function",
      },
      {
        inputs: [
          {
            internalType: "uint256",
            name: "_amount",
            type: "uint256",
          },
        ],
        name: "deposit",
        outputs: [],
        stateMutability: "nonpayable",
        type: "function",
      },
      {
        inputs: [
          {
            components: [
              {
                internalType: "string",
                name: "id",
                type: "string",
              },
            ],
            internalType: "struct TruQuest.ThingTd",
            name: "_thing",
            type: "tuple",
          },
          {
            internalType: "uint8",
            name: "_v",
            type: "uint8",
          },
          {
            internalType: "bytes32",
            name: "_r",
            type: "bytes32",
          },
          {
            internalType: "bytes32",
            name: "_s",
            type: "bytes32",
          },
        ],
        name: "fundThing",
        outputs: [],
        stateMutability: "nonpayable",
        type: "function",
      },
      {
        inputs: [
          {
            internalType: "address",
            name: "_user",
            type: "address",
          },
        ],
        name: "getAvailableFunds",
        outputs: [
          {
            internalType: "uint256",
            name: "",
            type: "uint256",
          },
        ],
        stateMutability: "view",
        type: "function",
      },
      {
        inputs: [
          {
            internalType: "address",
            name: "_user",
            type: "address",
          },
          {
            internalType: "uint256",
            name: "_amount",
            type: "uint256",
          },
        ],
        name: "reward",
        outputs: [],
        stateMutability: "nonpayable",
        type: "function",
      },
      {
        inputs: [],
        name: "s_acceptancePoll",
        outputs: [
          {
            internalType: "contract AcceptancePoll",
            name: "",
            type: "address",
          },
        ],
        stateMutability: "view",
        type: "function",
      },
      {
        inputs: [
          {
            internalType: "address",
            name: "",
            type: "address",
          },
        ],
        name: "s_balanceOf",
        outputs: [
          {
            internalType: "uint256",
            name: "",
            type: "uint256",
          },
        ],
        stateMutability: "view",
        type: "function",
      },
      {
        inputs: [
          {
            internalType: "address",
            name: "",
            type: "address",
          },
        ],
        name: "s_stakedBalanceOf",
        outputs: [
          {
            internalType: "uint256",
            name: "",
            type: "uint256",
          },
        ],
        stateMutability: "view",
        type: "function",
      },
      {
        inputs: [
          {
            internalType: "string",
            name: "",
            type: "string",
          },
        ],
        name: "s_thingSubmitter",
        outputs: [
          {
            internalType: "address",
            name: "",
            type: "address",
          },
        ],
        stateMutability: "view",
        type: "function",
      },
      {
        inputs: [],
        name: "s_verifierLottery",
        outputs: [
          {
            internalType: "contract VerifierLottery",
            name: "",
            type: "address",
          },
        ],
        stateMutability: "view",
        type: "function",
      },
      {
        inputs: [
          {
            internalType: "address",
            name: "_user",
            type: "address",
          },
          {
            internalType: "uint256",
            name: "_amount",
            type: "uint256",
          },
        ],
        name: "slash",
        outputs: [],
        stateMutability: "nonpayable",
        type: "function",
      },
      {
        inputs: [
          {
            internalType: "address",
            name: "_user",
            type: "address",
          },
          {
            internalType: "uint256",
            name: "_amount",
            type: "uint256",
          },
        ],
        name: "stake",
        outputs: [],
        stateMutability: "nonpayable",
        type: "function",
      },
      {
        inputs: [
          {
            internalType: "address",
            name: "_user",
            type: "address",
          },
          {
            internalType: "uint256",
            name: "_amount",
            type: "uint256",
          },
        ],
        name: "unstake",
        outputs: [],
        stateMutability: "nonpayable",
        type: "function",
      },
    ],
    functionName: "fundThing",
  });

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
        SignUpTd: signUpTd,
      },
      domain: domainData,
      primaryType: "SignUpTd",
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
        NewSubjectTd: newSubjectTd,
        TagTd: tagTd,
      },
      domain: domainData,
      primaryType: "NewSubjectTd",
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
        NewThingTd: newThingTd,
        EvidenceTd: evidenceTd,
        TagTd: tagTd,
      },
      domain: domainData,
      primaryType: "NewThingTd",
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
      setThingId(response.data.data.thing.id);
      setSig(response.data.data.signature.substring(2));
    }
  };

  const onFundThingTxnSent = async (txn) => {
    await txn.wait(1);
    console.log("Thing funded");
  };

  const fundThing = async () => {
    const r = "0x" + sig.substring(0, 64);
    const s = "0x" + sig.substring(64, 128);
    const v = parseInt(sig.substring(128, 130), 16);

    await runContractFunction({
      params: {
        params: {
          _thing: {
            id: thingId,
          },
          _v: v,
          _r: r,
          _s: s,
        },
      },
      onSuccess: onFundThingTxnSent,
      onError: (error) => console.error(error),
    });
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
      {thingId && <button onClick={fundThing}>Fund thing {thingId}</button>}
    </div>
  );
};

export default Home;
