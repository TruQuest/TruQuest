import 'package:flutter/material.dart';
import 'package:flutter/gestures.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:bot_toast/bot_toast.dart';

import 'ethereum/services/ethereum_rpc_provider.dart';
import 'general/services/local_storage.dart';
import 'widget_extensions.dart';
import 'injector.dart';
import 'general/pages/home_page.dart';

Future main() async {
  setup();

  await dotenv.load();

  var localStorage = resolveDependency<LocalStorage>();
  await localStorage.init();

  var ethereumRpcProvider = resolveDependency<EthereumRpcProvider>();
  await ethereumRpcProvider.init();

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

// class Dummy extends StatefulWidget {
//   const Dummy({
//     super.key,
//     required this.controller1,
//     required this.controller2,
//   });

//   final Controller controller1;
//   final Controller controller2;

//   @override
//   State<Dummy> createState() => _DummyState();
// }

// class _DummyState extends State<Dummy> {
//   final _dio = Dio(BaseOptions(baseUrl: 'http://localhost:5223'));
//   String? _email;

//   @override
//   void initState() {
//     super.initState();
//     html.window.addEventListener('message', _handleMessage);
//   }

//   @override
//   void dispose() {
//     html.window.removeEventListener('message', _handleMessage);
//     super.dispose();
//   }

//   void _handleMessage(html.Event e) {
//     if (e is html.MessageEvent && e.origin == 'http://localhost:5223') {
//       print('MessageEvent from ${e.origin}: ${e.data}');
//       // var response = await _dio.post(
//       //   '/dummy/save-share',
//       //   data: <String, dynamic>{
//       //     'email': email,
//       //     'keyShare': shares.first,
//       //   },
//       // );
//       // print('**************** Server key share saved!');
//     }
//   }

//   @override
//   Widget build(BuildContext context) {
//     return Scaffold(
//       body: SingleChildScrollView(
//         child: Column(
//           children: [
//             TextButton(
//               child: Text('Register'),
//               onPressed: () async {
//                 var email = 'maxmax.${DateTime.now().millisecondsSinceEpoch}@email.com';
//                 _email = email;

//                 var response = await _dio.post(
//                   '/dummy/create-user',
//                   data: <String, dynamic>{
//                     'email': email,
//                   },
//                 );

//                 var userId = response.data['data'] as String;
//                 print('*************** User created: $email $userId');

//                 response = await _dio.post(
//                   '/dummy/create-reg-options',
//                   data: <String, dynamic>{
//                     'email': email,
//                   },
//                 );

//                 var optionsMap = response.data['data'];

//                 print('***************\n\n' + JsonEncoder.withIndent('  ').convert(optionsMap));

//                 var options = AttestationOptions(
//                   rp: RelyingParty(
//                     id: optionsMap['rp']['id'],
//                     name: optionsMap['rp']['name'],
//                   ),
//                   user: User(
//                     id: optionsMap['user']['id'],
//                     name: optionsMap['user']['name'],
//                     displayName: optionsMap['user']['displayName'],
//                   ),
//                   challenge: optionsMap['challenge'],
//                   pubKeyCredParams: (optionsMap['pubKeyCredParams'] as List<dynamic>)
//                       .map(
//                         (submap) => PubKeyCredParam(
//                           type: submap['type'],
//                           alg: submap['alg'],
//                         ),
//                       )
//                       .toList(),
//                   timeout: optionsMap['timeout'],
//                   attestation: optionsMap['attestation'],
//                   authenticatorSelection: AuthenticatorSelection(
//                     authenticatorAttachment: optionsMap['authenticatorSelection']['authenticatorAttachment'],
//                     residentKey: optionsMap['authenticatorSelection']['residentKey'],
//                     requireResidentKey: optionsMap['authenticatorSelection']['requireResidentKey'],
//                     userVerification: optionsMap['authenticatorSelection']['userVerification'],
//                   ),
//                   // extensions: Extensions(
//                   //   prf: Prf(
//                   //     eval: Eval(
//                   //       first: optionsMap['extensions']['prf']['eval']['first'],
//                   //     ),
//                   //   ),
//                   // ),
//                 );

//                 var credential = await promiseToFuture<RawAttestation>(createCredential(options));
//                 print('******************** DONE!');

//                 response = await _dio.post(
//                   '/dummy/add-credential',
//                   data: <String, dynamic>{
//                     'userId': userId,
//                     'credential': {
//                       'id': credential.id,
//                       'rawId': credential.id,
//                       'type': credential.type,
//                       'response': {
//                         'attestationObject': credential.response.attestationObject,
//                         'clientDataJSON': credential.response.clientDataJSON,
//                       },
//                       'extensions': {},
//                     },
//                   },
//                 );
//                 print('**************** Cred added!');
//               },
//             ),
//             TextButton(
//               child: Text('Auth'),
//               onPressed: () async {
//                 var email = _email!;

//                 var response = await _dio.post(
//                   '/dummy/create-auth-options',
//                   data: <String, dynamic>{
//                     'email': email,
//                   },
//                 );

//                 var optionsMap = response.data['data'];

//                 print('***************\n\n' + JsonEncoder.withIndent('  ').convert(optionsMap));

//                 var options = CreateAuthOptions(
//                   rpId: optionsMap['rpId'],
//                   challenge: optionsMap['challenge'],
//                   allowCredentials: (optionsMap['allowCredentials'] as List<dynamic>)
//                       .map(
//                         (submap) => AllowCredential(
//                           type: submap['type'],
//                           id: submap['id'],
//                           // transports: submap['transports'],
//                         ),
//                       )
//                       .toList(),
//                   userVerification: optionsMap['userVerification'],
//                   timeout: optionsMap['timeout'],
//                 );

//                 var credential = await promiseToFuture<PublicKeyCredentialAssert>(getCredentials(options));
//                 print('******************** DONE!');

//                 response = await _dio.post(
//                   '/dummy/verify-credential',
//                   data: <String, dynamic>{
//                     'email': email,
//                     'credential': {
//                       'id': credential.id,
//                       'rawId': credential.id,
//                       'type': credential.type,
//                       'response': {
//                         'authenticatorData': credential.response.authenticatorData,
//                         'clientDataJSON': credential.response.clientDataJSON,
//                         'signature': credential.response.signature,
//                       },
//                       'extensions': {},
//                     },
//                   },
//                 );

//                 print('******************************** AAAAAAAAAAAAAAAAAA');
//               },
//             ),
//             TextButton(
//               child: Text('Generate key'),
//               onPressed: () {
//                 widget.controller1.postMessage('gen');
//               },
//             ),
//             TextButton(
//               child: Text('Scan key share'),
//               onPressed: () {
//                 widget.controller2.postMessage('scan');
//               },
//             ),
//             SizedBox(
//               width: 270,
//               height: 270,
//               child: HtmlElementView(viewType: widget.controller1.viewId),
//             ),
//             SizedBox(height: 20),
//             SizedBox(
//               width: 640,
//               height: 480,
//               child: HtmlElementView(viewType: widget.controller2.viewId),
//             ),
//           ],
//         ),
//       ),
//     );
//   }
// }
