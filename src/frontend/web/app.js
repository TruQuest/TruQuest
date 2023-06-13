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

async function getImagePalette(url) {
  var promise = new Promise((resolve, _) => {
    var image = new Image();
    image.crossOrigin = "Anonymous";
    image.src = url;

    image.onload = (_) => {
      console.log("Image loaded!");
      var colorThief = new ColorThief();
      var palette = colorThief.getPalette(image);
      resolve(palette);
    };
  });

  var palette = await promise;
  var colors = [];
  palette.forEach((color) => {
    colors.push({ red: color[0], green: color[1], blue: color[2] });
  });

  return { colors: colors };
}

class EthereumWallet {
  constructor() {}

  isInstalled() {
    return typeof ethereum !== "undefined";
  }

  instance() {
    return ethereum;
  }

  isInitialized() {
    return ethereum._state.initialized;
  }

  async getChainId() {
    try {
      var chainId = await ethereum.request({ method: "eth_chainId" });
      return {
        chainId: chainId,
        error: null,
      };
    } catch (error) {
      return {
        chainId: null,
        error: {
          code: error.code,
          message: error.message,
        },
      };
    }
  }

  async requestAccounts() {
    try {
      var accounts = await ethereum.request({ method: "eth_requestAccounts" });
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

  async getAccounts() {
    try {
      var accounts = await ethereum.request({ method: "eth_accounts" });
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

  async switchChain(chainParams) {
    var error = null;
    try {
      await ethereum.request({
        method: "wallet_switchEthereumChain",
        params: [{ chainId: chainParams.id }],
      });
    } catch (switchError) {
      if (switchError.code === 4902) {
        try {
          await ethereum.request({
            method: "wallet_addEthereumChain",
            params: [
              {
                chainId: chainParams.id,
                chainName: chainParams.name,
                rpcUrls: [chainParams.rpcUrl],
                nativeCurrency: {
                  name: "Ether",
                  symbol: "ETH",
                  decimals: 18,
                },
              },
            ],
          });
        } catch (addError) {
          error = addError;
        }
      } else {
        error = switchError;
      }
    }

    return {
      error:
        error != null
          ? {
              code: error.code,
              message: error.message,
            }
          : null,
    };
  }

  async signTypedData(account, data) {
    try {
      var signature = await ethereum.request({
        method: "eth_signTypedData_v4",
        params: [account, data],
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

  async personalSign(account, data) {
    try {
      var signature = await ethereum.request({
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

  removeAllListeners(event) {
    ethereum.removeAllListeners(event);
  }

  on(event, handler) {
    ethereum.on(event, handler);
  }
}

window.EthereumWallet = EthereumWallet;
