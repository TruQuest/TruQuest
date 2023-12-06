function coerceToArrayBuffer(thing, name) {
  if (typeof thing === "string") {
    // base64url to base64
    thing = thing.replace(/-/g, "+").replace(/_/g, "/");

    // base64 to Uint8Array
    var str = window.atob(thing);
    var bytes = new Uint8Array(str.length);
    for (var i = 0; i < str.length; i++) {
      bytes[i] = str.charCodeAt(i);
    }
    thing = bytes;
  }

  // Array to Uint8Array
  if (Array.isArray(thing)) {
    thing = new Uint8Array(thing);
  }

  // Uint8Array to ArrayBuffer
  if (thing instanceof Uint8Array) {
    thing = thing.buffer;
  }

  // error if none of the above worked
  if (!(thing instanceof ArrayBuffer)) {
    throw new TypeError("could not coerce '" + name + "' to ArrayBuffer");
  }

  return thing;
}

function coerceToBase64Url(thing) {
  // Array or ArrayBuffer to Uint8Array
  if (Array.isArray(thing)) {
    thing = Uint8Array.from(thing);
  }

  if (thing instanceof ArrayBuffer) {
    thing = new Uint8Array(thing);
  }

  // Uint8Array to base64
  if (thing instanceof Uint8Array) {
    var str = "";
    var len = thing.byteLength;

    for (var i = 0; i < len; i++) {
      str += String.fromCharCode(thing[i]);
    }
    thing = window.btoa(str);
  }

  if (typeof thing !== "string") {
    throw new Error("could not coerce to string");
  }

  // base64 to base64url
  // NOTE: "=" at the end of challenge is optional, strip it off here
  thing = thing.replace(/\+/g, "-").replace(/\//g, "_").replace(/=*$/g, "");

  return thing;
}

async function createCredential(options) {
  options.challenge = coerceToArrayBuffer(options.challenge);
  options.user.id = coerceToArrayBuffer(options.user.id);
  options.excludeCredentials.forEach((c) => {
    c.id = coerceToArrayBuffer(c.id);
  });
  // options.extensions.prf.eval.first = coerceToArrayBuffer(
  //   options.extensions.prf.eval.first
  // );

  var credential = null;
  try {
    credential = await navigator.credentials.create({
      publicKey: options,
    });
  } catch (error) {
    console.log(error);
  }

  if (credential == null) {
    return {
      attestation: null,
      error: "Something went wrong",
    };
  }

  var attestationObject = new Uint8Array(credential.response.attestationObject);
  var clientDataJSON = new Uint8Array(credential.response.clientDataJSON);

  return {
    attestation: {
      id: credential.id,
      type: credential.type,
      // extensions: credential.getClientExtensionResults(),
      response: {
        attestationObject: coerceToBase64Url(attestationObject),
        clientDataJSON: coerceToBase64Url(clientDataJSON),
      },
    },
    error: null,
  };
}

async function getCredential(options) {
  options.challenge = coerceToArrayBuffer(options.challenge);
  options.allowCredentials.forEach((allowCredential) => {
    allowCredential.id = coerceToArrayBuffer(allowCredential.id);
  });

  var credential = null;
  try {
    credential = await navigator.credentials.get({
      publicKey: options,
    });
  } catch (error) {
    console.log(error);
  }

  if (credential == null) {
    return {
      assertion: null,
      error: "Something went wrong",
    };
  }

  var authenticatorData = new Uint8Array(credential.response.authenticatorData);
  var clientDataJSON = new Uint8Array(credential.response.clientDataJSON);
  var signature = new Uint8Array(credential.response.signature);
  var userHandle = new Uint8Array(credential.response.userHandle);

  return {
    assertion: {
      id: credential.id,
      type: credential.type,
      // extensions: credential.getClientExtensionResults(),
      response: {
        authenticatorData: coerceToBase64Url(authenticatorData),
        clientDataJSON: coerceToBase64Url(clientDataJSON),
        signature: coerceToBase64Url(signature),
        userHandle: coerceToBase64Url(userHandle),
      },
    },
    error: null,
  };
}

async function fetchAndResizeImage(url) {
  const response = await fetch(url);
  var blob = await response.blob();
  const bitmap = await createImageBitmap(blob);

  const maxWidth = 1920.0;
  const maxHeight = 1080.0;

  console.log(`Width: ${bitmap.width}; Height: ${bitmap.height}`);

  var targetWidth = null;
  var targetHeight = null;
  if (bitmap.width > maxWidth) {
    targetWidth = maxWidth;
    const calcHeight = (targetHeight = Math.floor(
      bitmap.height * (targetWidth / bitmap.width)
    ));
    if (calcHeight > maxHeight) {
      targetHeight = maxHeight;
      targetWidth = Math.floor(targetWidth * (targetHeight / calcHeight));
    }
  } else if (bitmap.height > maxHeight) {
    targetHeight = maxHeight;
    const calcWidth = (targetWidth = Math.floor(
      bitmap.width * (targetHeight / bitmap.height)
    ));
    if (calcWidth > maxWidth) {
      targetWidth = maxWidth;
      targetHeight = Math.floor(targetHeight * (targetWidth / calcWidth));
    }
  }

  console.log(`Target width: ${targetWidth}; Target height: ${targetHeight}`);

  if (targetWidth != null && targetHeight != null) {
    const canvas = document.createElement("canvas");
    canvas.width = targetWidth;
    canvas.height = targetHeight;

    const _pica = pica();
    blob = await _pica.toBlob(await _pica.resize(bitmap, canvas), "image/jpeg");
  }

  const buffer = await blob.arrayBuffer();

  return { buffer: buffer, mimeType: blob.type };
}

// async function getImagePalette(url) {
//   var promise = new Promise((resolve, _) => {
//     var image = new Image();
//     image.crossOrigin = "Anonymous";
//     image.src = url;

//     image.onload = (_) => {
//       console.log("Image loaded!");
//       var colorThief = new ColorThief();
//       var palette = colorThief.getPalette(image);
//       resolve(palette);
//     };
//   });

//   var palette = await promise;
//   var colors = [];
//   palette.forEach((color) => {
//     colors.push({ red: color[0], green: color[1], blue: color[2] });
//   });

//   return { colors: colors };
// }

function retrieveUserOpStatusFromEvent(topics, data) {
  // @@HACK: ethers.utils.Interface.parseLog returns some unholy half-array half-map contraption,
  // which I have no idea how to bind to a Dart object, so do this instead...
  var abi = new ethers.utils.Interface(`[
    {
      "anonymous": false,
      "inputs": [
        {
          "indexed": true,
          "internalType": "bytes32",
          "name": "userOpHash",
          "type": "bytes32"
        },
        {
          "indexed": true,
          "internalType": "address",
          "name": "sender",
          "type": "address"
        },
        {
          "indexed": true,
          "internalType": "address",
          "name": "paymaster",
          "type": "address"
        },
        {
          "indexed": false,
          "internalType": "uint256",
          "name": "nonce",
          "type": "uint256"
        },
        {
          "indexed": false,
          "internalType": "bool",
          "name": "success",
          "type": "bool"
        },
        {
          "indexed": false,
          "internalType": "uint256",
          "name": "actualGasCost",
          "type": "uint256"
        },
        {
          "indexed": false,
          "internalType": "uint256",
          "name": "actualGasUsed",
          "type": "uint256"
        }
      ],
      "name": "UserOperationEvent",
      "type": "event"
    }
  ]`);

  var log = abi.parseLog({ topics, data });
  return {
    success: log.args.success,
    actualGasCost: log.args.actualGasCost.toHexString(),
    actualGasUsed: log.args.actualGasUsed.toHexString(),
  };
}

function retrieveUserOpRevertReasonFromEvent(topics, data) {
  // @@HACK: ethers.utils.Interface.parseLog returns some unholy half-array half-map contraption,
  // which I have no idea how to bind to a Dart object, so do this instead...
  var abi = new ethers.utils.Interface(`[
    {
      "anonymous": false,
      "inputs": [
        {
          "indexed": true,
          "internalType": "bytes32",
          "name": "userOpHash",
          "type": "bytes32"
        },
        {
          "indexed": true,
          "internalType": "address",
          "name": "sender",
          "type": "address"
        },
        {
          "indexed": false,
          "internalType": "uint256",
          "name": "nonce",
          "type": "uint256"
        },
        {
          "indexed": false,
          "internalType": "bytes",
          "name": "revertReason",
          "type": "bytes"
        }
      ],
      "name": "UserOperationRevertReason",
      "type": "event"
    }
  ]`);

  var log = abi.parseLog({ topics, data });
  return log.args.revertReason;
}

class EthereumWallet {
  provider;
  name;

  constructor() {
    this.provider = null;
    this.name = null;
  }

  async select(walletName, walletConnectProviderOpts) {
    // @@TODO: Check with both MM and CB installed and with only one wallet installed.
    if (walletName == "WalletConnect") {
      this.provider = await window[
        "@walletconnect/ethereum-provider"
      ].EthereumProvider.init(walletConnectProviderOpts);
    } else if (typeof ethereum !== "undefined") {
      if (ethereum.providers?.length) {
        for (var i = 0; i < ethereum.providers.length; ++i) {
          var provider = ethereum.providers[i];
          if (walletName == "Metamask" && provider.isMetaMask) {
            this.provider = provider;
            break;
          } else if (
            walletName == "CoinbaseWallet" &&
            provider.isCoinbaseWallet
          ) {
            this.provider = provider;
            break;
          }
        }
      } else if (
        (walletName == "Metamask" && ethereum.isMetaMask) ||
        (walletName == "CoinbaseWallet" && ethereum.isCoinbaseWallet)
      ) {
        this.provider = ethereum;
      }
    }

    if (this.provider != null) {
      this.name = walletName;
      return;
    }

    throw new Error(`${walletName} not available`);
  }

  isInitialized() {
    return this.name != "Metamask" || this.provider._state.initialized;
  }

  async requestAccounts(walletConnectConnectionOpts) {
    if (this.name == "WalletConnect") {
      // @@BUG: Awaiting this line never returns for some reason.
      this.provider.connect(walletConnectConnectionOpts);
      return {
        accounts: null,
        error: null,
      };
    } else {
      try {
        var accounts = await this.provider.request({
          method: "eth_requestAccounts",
        });

        return {
          accounts: accounts,
          error: null,
        };
      } catch (error) {
        return {
          accounts: null,
          error: {
            code: error.code,
            message: error.message,
          },
        };
      }
    }
  }

  async getAccounts() {
    // @@NOTE: Once expired the session is automatically removed on provider initialization.
    if (this.name == "WalletConnect" && this.provider.session == null) {
      return {
        accounts: [],
        error: null,
      };
    }

    try {
      var accounts = await this.provider.request({ method: "eth_accounts" });
      return {
        accounts: accounts,
        error: null,
      };
    } catch (error) {
      return {
        accounts: null,
        error: {
          code: error.code,
          message: error.message,
        },
      };
    }
  }

  // @@TODO: Provide asset params as function arg.
  async watchTruthserum() {
    try {
      var success = await this.provider.request({
        method: "wallet_watchAsset",
        params: {
          type: "ERC20",
          options: {
            address: "0x19CFc85e3dffb66295695Bf48e06386CB1B5f320",
            symbol: "TRU",
            decimals: 18,
            image: "https://svgshare.com/i/v42.svg",
          },
        },
      });

      return {
        error: success
          ? null
          : {
              code: 23666,
              message: "Something went wrong",
            },
      };
    } catch (error) {
      return {
        error: {
          code: error.code,
          message: error.message,
        },
      };
    }
  }

  async personalSign(account, data) {
    try {
      var signature = await this.provider.request({
        method: "personal_sign",
        params: [data, account],
      });

      return {
        signature: signature,
        error: null,
      };
    } catch (error) {
      return {
        signature: null,
        error: {
          code: error.code,
          message: error.message,
        },
      };
    }
  }

  // @@NOTE: Using removeListener instead of 'removeAllListeners' or 'off' since
  // WC's provider doesn't support 'removeAllListeners' and MM doesn't support 'off'.
  removeListener(event, handler) {
    this.provider.removeListener(event, handler);
  }

  on(event, handler) {
    this.provider.on(event, handler);
  }

  // @@NOTE: Only used by WC.
  once(event, handler) {
    this.provider.once(event, handler);
  }
}

window.EthereumWallet = EthereumWallet;

function signDigest(privateKey, digest) {
  var signingKey = new ethers.utils.SigningKey(privateKey);
  var hashedEthereumPrefixedMessage = ethers.utils.hashMessage(
    ethers.utils.arrayify(digest)
  );
  var signature = signingKey.signDigest(hashedEthereumPrefixedMessage);

  var serializedSignature =
    "0x" +
    signature.r.substring(2) +
    signature.s.substring(2) +
    (signature.v == 27 ? "1b" : "1c");

  return serializedSignature;
}
