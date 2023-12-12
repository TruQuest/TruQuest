import 'package:flutter/material.dart';
import 'package:flutter/gestures.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:bot_toast/bot_toast.dart';
import 'package:talker_flutter/talker_flutter.dart';
import 'package:logging/logging.dart' as logging;

import 'ethereum/services/ethereum_rpc_provider.dart';
import 'general/services/local_storage.dart';
import 'widget_extensions.dart';
import 'general/pages/home_page.dart';
import 'injector.dart';
import 'general/utils/logger.dart';

Future main() async {
  await dotenv.load();
  setup();

  var localStorage = resolveDependency<LocalStorage>();
  await localStorage.init();

  var ethereumRpcProvider = resolveDependency<EthereumRpcProvider>();
  await ethereumRpcProvider.init();

  logging.hierarchicalLoggingEnabled = true;

  logger = TalkerFlutter.init(
    logger: TalkerLogger(
      settings: TalkerLoggerSettings(level: LogLevel.debug),
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
      builder: BotToastInit(),
      navigatorObservers: [BotToastNavigatorObserver()],
      home: HomePage(),
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
