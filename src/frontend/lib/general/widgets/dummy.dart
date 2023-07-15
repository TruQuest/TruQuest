import 'dart:typed_data';

import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:convert/convert.dart';

import 'sign_in_stepper.dart';
import '../../ethereum/models/im/user_operation.dart';
import '../../ethereum/services/ethereum_api_service.dart';
import '../../ethereum_js_interop.dart';
import '../contracts/entrypoint_contract.dart';

class Dummy extends StatefulWidget {
  const Dummy({super.key});

  @override
  State<Dummy> createState() => _DummyState();
}

class _DummyState extends State<Dummy> {
  @override
  void initState() {
    super.initState();
    // foo();
  }

  void foo() async {
    var dummyAbi = '''
      [
          {
            "inputs": [],
            "name": "getValue",
            "outputs": [
              {
                "internalType": "string",
                "name": "",
                "type": "string"
              }
            ],
            "stateMutability": "view",
            "type": "function"
          },
          {
            "inputs": [
              {
                "internalType": "string",
                "name": "_value",
                "type": "string"
              }
            ],
            "name": "setValue",
            "outputs": [],
            "stateMutability": "nonpayable",
            "type": "function"
          }
      ]
    ''';

    var abi = Abi(dummyAbi);
    var callData = abi.encodeFunctionData(
      'setValue',
      [
        'Whatever you do, do it well',
      ],
    );

    var provider = JsonRpcProvider('http://localhost:8545');
    var dummyContract = Contract(
      '0x19CFc85e3dffb66295695Bf48e06386CB1B5f320',
      dummyAbi,
      provider,
    );
    var value = await dummyContract.read<String>('getValue');
    print('Dummy.getValue: $value');

    var walletAbi = '''
      [
          {
              "inputs": [
                  {
                      "internalType": "address",
                      "name": "dest",
                      "type": "address"
                  },
                  {
                      "internalType": "uint256",
                      "name": "value",
                      "type": "uint256"
                  },
                  {
                      "internalType": "bytes",
                      "name": "func",
                      "type": "bytes"
                  }
              ],
              "name": "execute",
              "outputs": [],
              "stateMutability": "nonpayable",
              "type": "function"
          }
      ]
    ''';

    abi = Abi(walletAbi);
    callData = abi.encodeFunctionData(
      'execute',
      [
        '0x19CFc85e3dffb66295695Bf48e06386CB1B5f320',
        0,
        callData,
      ],
    );

    print('CallData: $callData');

    var walletFactoryAbi = '''
      [
          {
              "inputs": [
                  {
                      "internalType": "address",
                      "name": "owner",
                      "type": "address"
                  },
                  {
                      "internalType": "uint256",
                      "name": "salt",
                      "type": "uint256"
                  }
              ],
              "name": "createAccount",
              "outputs": [
                  {
                      "internalType": "contract SimpleAccount",
                      "name": "ret",
                      "type": "address"
                  }
              ],
              "stateMutability": "nonpayable",
              "type": "function"
          },
          {
              "inputs": [
                  {
                      "internalType": "address",
                      "name": "owner",
                      "type": "address"
                  },
                  {
                      "internalType": "uint256",
                      "name": "salt",
                      "type": "uint256"
                  }
              ],
              "name": "getAddress",
              "outputs": [
                  {
                      "internalType": "address",
                      "name": "",
                      "type": "address"
                  }
              ],
              "stateMutability": "view",
              "type": "function"
          }
      ]
    ''';

    var owner = '0x9C828E3ddD81A9512BBAB6CA1A245278BF0E45Da';
    var walletFactoryAddress = '0x9406Cc6185a346906296840746125a0E44976454';

    var walletFactoryContract = Contract(
      walletFactoryAddress,
      walletFactoryAbi,
      provider,
    );

    var walletAddress = await walletFactoryContract.read<String>(
      'getAddress',
      args: [owner, 0],
    );

    print('Wallet Address: $walletAddress');

    abi = Abi(walletFactoryAbi);
    var initCode = abi.encodeFunctionData(
      'createAccount',
      [
        owner,
        0,
      ],
    );

    initCode = walletFactoryAddress + initCode.substring(2);
    initCode = '0x';

    print('InitCode: $initCode');

    var api = EthereumApiService();

    var entryPointAddress = await api.getEntryPointAddress();
    print('EntryPoint: $entryPointAddress');
    var entryPointContract = EntryPointContract(entryPointAddress!);

    var nonce = await entryPointContract.getNonce(walletAddress, 0);
    print('Nonce: $nonce');

    // var minPriorityFeeBid = BigInt.parse('1000000000');

    var baseFee = await api.getBaseFee();
    print('Base fee: 0x${baseFee!.toRadixString(16)} WEI');

    // var maxPriorityFee = await api.getMaxPriorityFee();
    // print('Max priority fee: 0x${maxPriorityFee!.toRadixString(16)} WEI');

    // var maxPriorityFeeBid = BigInt.from(
    //   (maxPriorityFee * BigInt.from(4)) / BigInt.from(3),
    // );
    // assert(maxPriorityFeeBid >= maxPriorityFee);
    // if (maxPriorityFeeBid < minPriorityFeeBid) {
    //   maxPriorityFeeBid = minPriorityFeeBid;
    // }
    // print('Max priority fee bid: 0x${maxPriorityFeeBid.toRadixString(16)} WEI');
    // print(
    //   'Max priority fee bid / Max priority fee = ${maxPriorityFeeBid / maxPriorityFee}',
    // );

    var maxPriorityFeeBid = BigInt.zero;
    var maxFeeBid = baseFee * BigInt.two + maxPriorityFeeBid;
    print('Max fee bid: 0x${maxFeeBid.toRadixString(16)} WEI');

    var dummySignature =
        '0xfffffffffffffffffffffffffffffff0000000000000000000000000000000007aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa1c';

    // setting both MAXes to non-zero returns error "AA21 didn't pay prefund"
    var userOp = UserOperation(
      sender: walletAddress,
      nonce: nonce,
      initCode: initCode,
      callData: callData,
      callGasLimit: BigInt.zero,
      verificationGasLimit: BigInt.zero,
      preVerificationGas: BigInt.zero,
      maxFeePerGas: maxFeeBid,
      maxPriorityFeePerGas: maxPriorityFeeBid,
      signature: dummySignature,
    );

    var fees = await api.estimateUserOperationGas(userOp, entryPointAddress);
    print(fees!);

    userOp = userOp.copyWith(
      callGasLimit: fees['callGasLimit']! * BigInt.two,
      verificationGasLimit: fees['verificationGasLimit']! * BigInt.two,
      preVerificationGas: fees['preVerificationGas']! * BigInt.two,
    );

    var estimatedGasCost = (userOp.preVerificationGas +
            userOp.verificationGasLimit +
            userOp.callGasLimit) *
        userOp.maxFeePerGas;

    print('Estimated gas cost: $estimatedGasCost WEI');
    print(
      'Estimated gas cost: ${formatUnits(BigNumber.from(estimatedGasCost.toString()))} ETH',
    );

    String? userOpHash = await entryPointContract.getUserOpHash(userOp);
    print('UserOp Hash: $userOpHash');

    var msgHash = hashMessage(
      Uint8List.fromList(hex.decode(userOpHash.substring(2))),
    );
    print('Msg Hash: $msgHash');

    var signingKey = SigningKey(
      Uint8List.fromList(hex.decode(dotenv.env['DUMMY_OWNER_PRIVATE_KEY']!)),
    );
    var signature = signingKey.signDigest(
      Uint8List.fromList(hex.decode(msgHash.substring(2))),
    );
    print('Signature: ${signature.combined}');

    userOp = userOp.copyWith(signature: signature.combined);
    print('UserOp:\n${userOp.toJson()}');

    return;

    print('\n\nSending UserOp...');

    userOpHash = await api.sendUserOperation(userOp, entryPointAddress);
    print('UserOp Hash: $userOpHash');
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: IconButton(
          icon: Icon(Icons.add),
          onPressed: () {
            showDialog(
              context: context,
              builder: (context) => SignInStepper(),
            );
          },
        ),
      ),
    );
  }
}
