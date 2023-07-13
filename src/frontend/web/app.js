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

async function initWalletConnectProvider(walletConnectProviderOpts) {
  window.truquest_walletConnectProvider = await window[
    "@walletconnect/ethereum-provider"
  ].EthereumProvider.init(walletConnectProviderOpts);
}

class EthereumWallet {
  provider;
  name;

  constructor() {
    this.provider = null;
    this.name = null;
  }

  select(walletName) {
    // @@TODO: Check with both MM and CB installed and with only one wallet installed.
    if (walletName == "WalletConnect") {
      this.provider = window.truquest_walletConnectProvider;
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

  instance() {
    return this.provider;
  }

  isInitialized() {
    return this.name != "Metamask" || this.provider._state.initialized;
  }

  walletConnectSessionExists() {
    // @@NOTE: Once expired the session is automatically removed on provider initialization.
    return this.provider.session != null;
  }

  async getChainId() {
    try {
      var chainId = await this.provider.request({ method: "eth_chainId" });
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

  async requestAccounts(walletConnectConnectionOpts) {
    if (this.name == "WalletConnect") {
      // @@BUG: Awaiting this line never returns for some reason.
      this.provider.connect(walletConnectConnectionOpts);
      return {
        error: null,
      };
    } else {
      try {
        await this.provider.request({
          method: "eth_requestAccounts",
        });

        return {
          error: null,
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
  }

  async getAccounts() {
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

  async switchChain(chainParams) {
    var error = null;
    try {
      // @@NOTE: In WalletConnect calling this when the chain exists simply changes the
      // local chainId value ('chainChanged' gets fired), but no request is sent to the app,
      // meaning, it stays on the old chain.
      await this.provider.request({
        method: "wallet_switchEthereumChain",
        params: [{ chainId: chainParams.id }],
      });
    } catch (switchError) {
      if (
        (this.name == "Metamask" && switchError.code === 4902) ||
        ((this.name == "CoinbaseWallet" || this.name == "WalletConnect") &&
          switchError.code === -32603)
      ) {
        try {
          await this.provider.request({
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

  async signTypedData(account, data) {
    try {
      var signature = await this.provider.request({
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
