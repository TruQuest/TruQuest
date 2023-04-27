import 'dart:async';

import 'package:dio/dio.dart';
import 'package:logging/logging.dart';
import 'package:signalr_netcore/signalr_client.dart';
import 'package:tuple/tuple.dart';

import '../../settlement/models/rvm/settlement_proposal_state_vm.dart';
import '../../thing/models/rvm/thing_state_vm.dart';

enum ServerEventType {
  thing,
  settlement,
}

enum ThingEventType {
  draftCreationProgress,
  draftCreated,
  stateChanged,
}

enum SettlementEventType {
  draftCreationProgress,
  draftCreated,
  stateChanged,
}

class ServerConnector {
  late final Dio dio;

  HubConnection? _hubConnection;
  HubConnection? get hubConnection => _hubConnection;

  String? _accessToken;
  String? get accessToken => _accessToken;

  final StreamController<Future Function()> _hubConnectionTaskQueue =
      StreamController<Future Function()>();

  final StreamController<Tuple2<ServerEventType, Object>> _serverEventChannel =
      StreamController<Tuple2<ServerEventType, Object>>.broadcast();
  Stream<Tuple2<ServerEventType, Object>> get serverEvent$ =>
      _serverEventChannel.stream;

  ServerConnector() {
    Logger.root.level = Level.ALL;
    Logger.root.onRecord.listen((LogRecord rec) {
      print('[${rec.level.name}]: ${rec.time}: ${rec.message}');
    });

    dio = Dio(
      BaseOptions(
        baseUrl: 'http://localhost:5223',
      ),
    );

    _monitorHubConnectionTasks();
  }

  void _monitorHubConnectionTasks() async {
    // @@??: debounce?
    var iterator = StreamIterator(_hubConnectionTaskQueue.stream);
    while (await iterator.moveNext()) {
      var task = iterator.current;
      await task();
    }
  }

  Future _connectToHub(String? token) async {
    _accessToken = token;
    var hubConnection = _hubConnection;
    if (hubConnection != null) {
      try {
        await hubConnection.stop();
      } catch (e) {
        print(e);
      }
      _hubConnection = null;
    }

    var hubLogger = Logger('Hub');
    var transportLogger = Logger('Transport');

    hubConnection = _hubConnection = HubConnectionBuilder()
        .withUrl(
          'http://localhost:5223/hub',
          options: HttpConnectionOptions(
            transport: HttpTransportType.WebSockets,
            logger: transportLogger,
            logMessageContent: true,
            accessTokenFactory:
                token != null ? () => Future.value(token) : null,
          ),
        )
        .configureLogging(hubLogger)
        .build();

    hubConnection.keepAliveIntervalInMilliseconds = 90 * 1000;
    hubConnection.serverTimeoutInMilliseconds = 180 * 1000;

    hubConnection.onclose(({error}) {
      print('HubConnection closed. Error: $error');
    });

    hubConnection.on(
      'TellAboutNewThingDraftCreationProgress',
      (List<Object?>? args) {
        var thingId = args!.first as String;
        _serverEventChannel.add(
          Tuple2(
            ServerEventType.thing,
            Tuple3(
              ThingEventType.draftCreationProgress,
              thingId,
              args.last!,
            ),
          ),
        );
      },
    );

    hubConnection.on(
      'NotifyThingStateChanged',
      (List<Object?>? args) {
        var thingId = args!.first as String;
        Object state = ThingStateVm.values[args.last as int];
        _serverEventChannel.add(
          Tuple2(
            ServerEventType.thing,
            Tuple3(
              ThingEventType.stateChanged,
              thingId,
              state,
            ),
          ),
        );
      },
    );

    hubConnection.on(
      'TellAboutNewSettlementProposalDraftCreationProgress',
      (List<Object?>? args) {
        var proposalId = args!.first as String;
        _serverEventChannel.add(
          Tuple2(
            ServerEventType.settlement,
            Tuple3(
              SettlementEventType.draftCreationProgress,
              proposalId,
              args.last!,
            ),
          ),
        );
      },
    );

    hubConnection.on(
      'NotifySettlementProposalStateChanged',
      (List<Object?>? args) {
        var proposalId = args!.first as String;
        Object state = SettlementProposalStateVm.values[args.last as int];
        _serverEventChannel.add(
          Tuple2(
            ServerEventType.settlement,
            Tuple3(
              SettlementEventType.stateChanged,
              proposalId,
              state,
            ),
          ),
        );
      },
    );

    await hubConnection.start(); // @@TODO: Handle error.
  }

  Future _disconnectFromHub() async {
    _accessToken = null;
    if (_hubConnection != null) {
      try {
        await _hubConnection!.stop();
      } catch (e) {
        print(e);
      }
      _hubConnection = null;
    }
  }

  void disconnectFromHub() => _hubConnectionTaskQueue.add(_disconnectFromHub);

  void connectToHub(String? accessToken) =>
      _hubConnectionTaskQueue.add(() => _connectToHub(accessToken));
}
