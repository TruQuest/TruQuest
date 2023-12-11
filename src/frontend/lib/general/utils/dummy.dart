import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';

import 'logger.dart';
import 'utils.dart';
import '../../ethereum/errors/user_operation_error.dart';
import '../../ethereum/models/im/user_operation.dart';
import '../../ethereum/services/ethereum_api_service.dart';
import '../../ethereum/services/ethereum_rpc_provider.dart';
import '../../ethereum/services/user_operation_service.dart';
import '../../ethereum_js_interop.dart';
import '../../user/models/vm/user_vm.dart';
import '../../widget_extensions.dart';
import '../contexts/multi_stage_operation_context.dart';
import '../contracts/base_contract.dart';
import '../contracts/erc4337/iaccount_factory_contract.dart';
import '../contracts/erc4337/ientrypoint_contract.dart';

class Dummy extends StatefulWidget {
  const Dummy({super.key});

  @override
  State<Dummy> createState() => _DummyState();
}

class _DummyState extends StateX<Dummy> {
  late final _userServiceDummy = use<UserServiceDummy>();
  late final _accountFactoryContract = use<IAccountFactoryContract>();
  late final _userOperationService = use<UserOperationService>();
  late final _dummyContractOld = use<DummyContractOld>();
  late final _dummyContract = use<DummyContract>();

  String? _name;
  int? _age;

  @override
  void initState() {
    super.initState();
    _init();
  }

  void _init() async {
    var signerAddress = dotenv.env['DUMMY_OWNER_ADDRESS']!;
    var walletAddress = await _accountFactoryContract.getAddress(signerAddress);
    logger.info('******** Signer: $signerAddress. Wallet: $walletAddress ***************');

    _userServiceDummy.setUser(
      UserVm(
        originWallet: 'Embedded',
        id: 'asdasd',
        signerAddress: signerAddress,
        walletAddress: walletAddress,
      ),
    );
  }

  Stream<Object> _foo(String name, int age, String data, MultiStageOperationContext ctx) async* {
    logger.info('+++++++++++++++++++++++++++++ foo +++++++++++++++++++++++++++++++++++++++');
    yield _userOperationService.prepareOneWithRealTimeFeeUpdates(
      actions: [(_dummyContract, _dummyContract.foo(name, age, data))],
      functionSignature: 'foo(...)',
      description: 'Just foo',
    );

    var userOp = await ctx.approveUserOpTask.future;
    if (userOp == null) return;

    var error = await _userOperationService.send(userOp);
    if (error != null) yield error;
  }

  Stream<Object> _mooFoo(String name, int age, String data, MultiStageOperationContext ctx) async* {
    logger.info('+++++++++++++++++++++++++++++ mooFoo +++++++++++++++++++++++++++++++++++++++');
    yield _userOperationService.prepareOneWithRealTimeFeeUpdates(
      actions: [
        (_dummyContract, _dummyContract.moo(age)),
        (_dummyContractOld, _dummyContractOld.foo(name, age, data)),
      ],
      functionSignature: 'moo(...); foo(...) (old)',
      description: ' moo and foo (old)',
    );

    var userOp = await ctx.approveUserOpTask.future;
    if (userOp == null) return;

    var error = await _userOperationService.send(userOp);
    if (error != null) yield error;
  }

  @override
  Widget buildX(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Dummy')),
      body: Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            SizedBox(
              width: 300,
              child: TextField(
                onChanged: (value) => _name = value,
                decoration: InputDecoration(hintText: 'Name'),
              ),
            ),
            SizedBox(height: 20),
            SizedBox(
              width: 300,
              child: TextField(
                onChanged: (value) {
                  int? age;
                  if ((age = int.tryParse(value)) != null) _age = age;
                },
                decoration: InputDecoration(hintText: 'Age'),
              ),
            ),
            SizedBox(height: 20),
            ElevatedButton(
              child: Text('Foo'),
              onPressed: () async {
                if (_name == null || _age == null) return;

                logger.info('Foo: $_name $_age');

                var success = await multiStageFlow(
                  context,
                  (ctx) => _foo(
                    _name!,
                    _age!,
                    '0x1122112211221122112211221122112211221122112211221122112211221122',
                    ctx,
                  ),
                );
                if (!success)
                  logger.info('---------------------- FOO FAILED ------------------------');
                else
                  logger.info('name: ${await _dummyContract.name()}');
              },
            ),
            SizedBox(height: 20),
            ElevatedButton(
              child: Text('MooFoo'),
              onPressed: () async {
                if (_name == null || _age == null) return;

                logger.info('MooFoo: $_name $_age');

                var success = await multiStageFlow(
                  context,
                  (ctx) => _mooFoo(
                    _name!,
                    _age!,
                    '0x1122112211221122112211221122112211221122112211221122112211221122',
                    ctx,
                  ),
                );
                if (!success)
                  logger.info('---------------------- MOOFOO FAILED ------------------------');
                else
                  logger.info('age: ${await _dummyContract.age()}');
              },
            ),
          ],
        ),
      ),
    );
  }
}

class DummyContractOld extends BaseContract {
  static const String _address = '0x011B8676A0C240c1f978FCcBb8DE3eb02fec8FB8';
  static const String _abi = '''
[
    {
        "inputs": [
            {
                "internalType": "address",
                "name": "sender",
                "type": "address"
            }
        ],
        "name": "Dummy__Unauthorized",
        "type": "error"
    },
    {
        "inputs": [
            {
                "internalType": "uint16",
                "name": "age",
                "type": "uint16"
            }
        ],
        "name": "Dummy__FooFoo",
        "type": "error"
    },
    {
        "inputs": [
            {
                "internalType": "uint16",
                "name": "age",
                "type": "uint16"
            }
        ],
        "name": "Dummy__MooMoo",
        "type": "error"
    },
    {
        "anonymous": false,
        "inputs": [
            {
                "indexed": true,
                "internalType": "string",
                "name": "name",
                "type": "string"
            },
            {
                "indexed": false,
                "internalType": "uint16",
                "name": "age",
                "type": "uint16"
            },
            {
                "indexed": false,
                "internalType": "bytes32",
                "name": "data",
                "type": "bytes32"
            }
        ],
        "name": "Fooed",
        "type": "event"
    },
    {
        "anonymous": false,
        "inputs": [
            {
                "indexed": false,
                "internalType": "uint16",
                "name": "age",
                "type": "uint16"
            }
        ],
        "name": "Mooed",
        "type": "event"
    },
    {
        "inputs": [
            {
                "internalType": "string",
                "name": "name",
                "type": "string"
            },
            {
                "internalType": "uint16",
                "name": "age",
                "type": "uint16"
            },
            {
                "internalType": "bytes32",
                "name": "data",
                "type": "bytes32"
            }
        ],
        "name": "foo",
        "outputs": [],
        "stateMutability": "nonpayable",
        "type": "function"
    },
    {
        "inputs": [
            {
                "internalType": "uint16",
                "name": "age",
                "type": "uint16"
            }
        ],
        "name": "moo",
        "outputs": [],
        "stateMutability": "nonpayable",
        "type": "function"
    },
    {
        "inputs": [],
        "name": "s_age",
        "outputs": [
            {
                "internalType": "uint16",
                "name": "",
                "type": "uint16"
            }
        ],
        "stateMutability": "view",
        "type": "function"
    },
    {
        "inputs": [],
        "name": "s_name",
        "outputs": [
            {
                "internalType": "string",
                "name": "",
                "type": "string"
            }
        ],
        "stateMutability": "view",
        "type": "function"
    }
]
''';

  DummyContractOld(EthereumRpcProvider ethereumRpcProvider) : super(_address, _abi, ethereumRpcProvider.provider);

  Future<String> name() => contract.read<String>('s_name');
  Future<int> age() => contract.read<int>('s_age');

  String foo(String name, int age, String data) {
    return interface.encodeFunctionData('foo', [name, age, data]);
  }

  String moo(int age) {
    return interface.encodeFunctionData('moo', [age]);
  }
}

class DummyContract extends BaseContract {
  static const String _address = '0xDC122351BDBD0CAc3F81e0A6CAaB86b0eC623ebD';
  static const String _abi = '''
[
    {
        "inputs": [
            {
                "internalType": "uint16",
                "name": "age",
                "type": "uint16"
            }
        ],
        "name": "Dummy__FooFoo",
        "type": "error"
    },
    {
        "inputs": [
            {
                "internalType": "uint16",
                "name": "age",
                "type": "uint16"
            }
        ],
        "name": "Dummy__MooMoo",
        "type": "error"
    },
    {
        "anonymous": false,
        "inputs": [
            {
                "indexed": true,
                "internalType": "string",
                "name": "name",
                "type": "string"
            },
            {
                "indexed": false,
                "internalType": "uint16",
                "name": "age",
                "type": "uint16"
            },
            {
                "indexed": false,
                "internalType": "bytes32",
                "name": "data",
                "type": "bytes32"
            }
        ],
        "name": "Fooed",
        "type": "event"
    },
    {
        "anonymous": false,
        "inputs": [
            {
                "indexed": false,
                "internalType": "uint16",
                "name": "age",
                "type": "uint16"
            }
        ],
        "name": "Mooed",
        "type": "event"
    },
    {
        "inputs": [
            {
                "internalType": "string",
                "name": "name",
                "type": "string"
            },
            {
                "internalType": "uint16",
                "name": "age",
                "type": "uint16"
            },
            {
                "internalType": "bytes32",
                "name": "data",
                "type": "bytes32"
            }
        ],
        "name": "foo",
        "outputs": [],
        "stateMutability": "nonpayable",
        "type": "function"
    },
    {
        "inputs": [
            {
                "internalType": "uint16",
                "name": "age",
                "type": "uint16"
            }
        ],
        "name": "moo",
        "outputs": [],
        "stateMutability": "nonpayable",
        "type": "function"
    },
    {
        "inputs": [],
        "name": "s_age",
        "outputs": [
            {
                "internalType": "uint16",
                "name": "",
                "type": "uint16"
            }
        ],
        "stateMutability": "view",
        "type": "function"
    },
    {
        "inputs": [],
        "name": "s_name",
        "outputs": [
            {
                "internalType": "string",
                "name": "",
                "type": "string"
            }
        ],
        "stateMutability": "view",
        "type": "function"
    }
]
''';

  DummyContract(EthereumRpcProvider ethereumRpcProvider) : super(_address, _abi, ethereumRpcProvider.provider);

  Future<String> name() => contract.read<String>('s_name');
  Future<int> age() => contract.read<int>('s_age');

  String foo(String name, int age, String data) {
    return interface.encodeFunctionData('foo', [name, age, data]);
  }

  String moo(int age) {
    return interface.encodeFunctionData('moo', [age]);
  }
}

class UserOperationBuilderDummy {
  final UserServiceDummy _userService;
  final EthereumApiService _ethereumApiService;
  final IEntryPointContract _entryPointContract;
  final IAccountFactoryContract _accountFactoryContract;

  late final String _sender;
  late final BigInt _nonce;
  late final String _initCode;
  late final String _callData;
  String _paymasterAndData = '0x';

  late UserOperation _userOp;

  final List<Future Function()> _tasks = [];

  late final BigInt estimatedGasCost;

  UserOperationBuilderDummy(
    this._userService,
    this._ethereumApiService,
    this._entryPointContract,
    this._accountFactoryContract,
  ) {
    _sender = _userService.latestCurrentUser.walletAddress!;
    _tasks.add(() async {
      var code = await _ethereumApiService.getCode(_sender);
      _initCode =
          code == '0x' ? _accountFactoryContract.getInitCode(_userService.latestCurrentUser.signerAddress!) : '0x';
    });
  }

  UserOperationBuilderDummy._(
    this._userService,
    this._ethereumApiService,
    this._entryPointContract,
    this._accountFactoryContract,
    this._sender,
    this._initCode,
    this._callData,
  );

  UserOperationBuilderDummy _withCurrentNonce() {
    _tasks.add(() async {
      _nonce = await _entryPointContract.getNonce(_sender);
    });

    return this;
  }

  UserOperationBuilderDummy action((String, String) targetAndCallData) {
    _callData = _accountFactoryContract.accountContract.execute(targetAndCallData);
    return this;
  }

  UserOperationBuilderDummy actions(List<(String, String)> targetAndCallDataList) {
    _callData = _accountFactoryContract.accountContract.executeBatch(targetAndCallDataList);
    return this;
  }

  UserOperationBuilderDummy _withEstimatedGasLimits() {
    _tasks.add(() async {
      _userOp = UserOperation(
        sender: _sender,
        nonce: _nonce,
        initCode: _initCode,
        callData: _callData,
        callGasLimit: BigInt.zero,
        verificationGasLimit: BigInt.zero,
        preVerificationGas: BigInt.zero,
        maxFeePerGas: BigInt.zero,
        maxPriorityFeePerGas: BigInt.zero,
        paymasterAndData: _paymasterAndData,
        signature: _accountFactoryContract.dummySignatureForGasEstimation,
      );

      var fees = await _ethereumApiService.estimateUserOperationGas(_userOp);
      var (preVerificationGas, verificationGasLimit, callGasLimit) = fees;

      logger.info('PreVerificationGas:\n\t$preVerificationGas');
      logger.info('VerificationGasLimit:\n\t$verificationGasLimit');
      logger.info('CallGasLimit:\n\t$callGasLimit');

      _userOp = _userOp.copyWith(
        callGasLimit: callGasLimit,
        verificationGasLimit: verificationGasLimit,
        preVerificationGas: preVerificationGas,
      );
    });

    return this;
  }

  UserOperationBuilderDummy _withCurrentGasPrice() {
    _tasks.add(() async {
      /*
        Source: https://docs.alchemy.com/reference/bundler-api-fee-logic

        Recommended Actions for Calculating maxFeePerGas:
            - Fetch Current Base Fee: Use the method eth_getBlockByNumber with the 'latest' parameter to get the current baseFeePerGas.
            - Apply Buffer on Base Fee: To account for potential fee changes, apply a buffer on the current base fee
            based on the requirements in the table shown above (5% for Arbitrum Mainnet and 50% for all other mainnets)
            - Fetch Current Priority Fee with Rundler: Use the rundler_maxPriorityFeePerGas method to query the current priority fee for the network.
            - Apply Buffer on Priority Fee: Once you have the current priority fee using rundler_maxPriorityFeePerGas,
            increase it according to the fee requirement table shown above for any unexpected changes (No buffer for Arbitrum Mainnet and
            25% buffer for all other mainnets).
            - Determine maxFeePerGas: Add the buffered values from steps 2 and 4 together to obtain the maxFeePerGas for your user operation.
      */

      var baseFee = await _ethereumApiService.getBaseFee();
      if (baseFee == null) throw UserOperationError(message: 'Error trying to get current base fee');

      logger.info('Base fee:\n\t0x${baseFee.toRadixString(16)} WEI');
      baseFee = BigInt.from((baseFee * BigInt.from(3)) / BigInt.two);
      logger.info('Base fee bid (+ 50% buffer):\n\t0x${baseFee.toRadixString(16)} WEI');

      var maxPriorityFee = await _ethereumApiService.getMaxPriorityFee();
      if (maxPriorityFee == null) throw UserOperationError(message: 'Error trying to get current max priority fee');

      logger.info('Max priority fee:\n\t0x${maxPriorityFee.toRadixString(16)} WEI');
      maxPriorityFee = BigInt.from((maxPriorityFee * BigInt.from(5)) / BigInt.from(4));
      logger.info('Max priority fee bid (+ 25% buffer):\n\t0x${maxPriorityFee.toRadixString(16)} WEI');

      var maxFeeBid = baseFee + maxPriorityFee;
      logger.info('Max fee bid:\n\t0x${maxFeeBid.toRadixString(16)} WEI');

      _userOp = _userOp.copyWith(
        maxFeePerGas: maxFeeBid,
        maxPriorityFeePerGas: maxPriorityFee,
      );

      estimatedGasCost = _userOp.totalProvisionedGas * _userOp.maxFeePerGas;

      logger.info('Estimated gas cost: ${formatUnits(BigNumber.from(estimatedGasCost.toString()))} ETH');
    });

    return this;
  }

  Future<UserOperation> unsigned() async {
    _withCurrentNonce()._withEstimatedGasLimits()._withCurrentGasPrice();

    for (var task in _tasks) await task();

    // _userOp.builder = this;

    return _userOp;
  }

  Future<UserOperation> signed() async {
    _withCurrentNonce()._withEstimatedGasLimits()._withCurrentGasPrice();

    _tasks.add(() async {
      var userOpHash = await _entryPointContract.getUserOpHash(_userOp);
      _userOp = _userOp.copyWith(
        signature: await _userService.personalSignDigest(userOpHash),
      );
    });

    for (var task in _tasks) await task();

    return _userOp;
  }
}

class UserServiceDummy {
  late final UserVm _user;
  UserVm get latestCurrentUser => _user;

  void setUser(UserVm user) => _user = user;

  Future<String> personalSignDigest(String digest) async {
    return signDigest(dotenv.env['DUMMY_OWNER_PRIVATE_KEY']!, digest);
  }
}
