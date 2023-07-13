import 'dart:js_util';

import 'package:flutter/material.dart';
import 'package:flutter/gestures.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';

import 'ethereum_js_interop.dart';
import 'general/services/local_storage.dart';
import 'widget_extensions.dart';
import 'injector.dart';
import 'general/pages/home_page.dart';

Future main() async {
  setup();

  await dotenv.load();

  var localStorage = resolveDependency<LocalStorage>();
  await localStorage.init();

  // @@!!: Dirty hack to avoid making EthereumService's ctor do async stuff,
  // since it would lead to all sorts of complications down the line.
  await promiseToFuture(
    initWalletConnectProvider(
      WalletConnectProviderOpts(
        projectId: dotenv.env['WALLET_CONNECT_PROJECT_ID']!,
        chains: [1],
        optionalChains: [901],
        rpcMap: jsify({
          '901': 'http://localhost:9545',
        }),
        showQrModal: false,
        methods: [
          'personal_sign', // Checked in MM, works.
          'eth_sendTransaction',
          'eth_signTypedData_v4',
          'wallet_addEthereumChain', // Trust Wallet doesn't support it. MM does.
          'wallet_watchAsset',
          "wallet_scanQRCode",
        ],
        events: [
          'chainChanged',
          'accountsChanged',
        ],
      ),
    ),
  );

  runApp(const App());
}

class App extends StatelessWidget {
  const App({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      debugShowCheckedModeBanner: false,
      scrollBehavior: MouseIncludedScrollBehavior(),
      title: 'TruQuest',
      theme: ThemeData(
        primarySwatch: Colors.blue,
      ),
      home: const HomePage(),
    );
  }
}

class MouseIncludedScrollBehavior extends MaterialScrollBehavior {
  @override
  Set<PointerDeviceKind> get dragDevices => {
        PointerDeviceKind.touch,
        PointerDeviceKind.mouse,
      };
}
