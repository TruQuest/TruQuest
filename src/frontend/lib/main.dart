import 'package:flutter/material.dart';
import 'package:flutter/gestures.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';

import 'general/services/local_storage.dart';
import 'widget_extensions.dart';
import 'injector.dart';
import 'general/pages/home_page.dart';

Future main() async {
  setup();

  await dotenv.load();

  var localStorage = resolveDependency<LocalStorage>();
  await localStorage.init();

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
