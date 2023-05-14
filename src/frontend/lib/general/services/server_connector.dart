import 'dart:async';

import 'package:dio/dio.dart';
import 'package:logging/logging.dart';
import 'package:signalr_netcore/signalr_client.dart';
import 'package:tuple/tuple.dart';

import '../models/rvm/notification_vm.dart';
import '../models/rvm/watched_item_type_vm.dart';

enum ServerEventType {
  connected,
  notification,
  thing,
  settlement,
}

enum NotificationEventType {
  initialRetrieve,
  newOne,
}

enum ThingEventType {
  draftCreationProgress,
}

enum SettlementEventType {
  draftCreationProgress,
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
      'NotifyAboutItemUpdate',
      (List<Object?>? args) {
        var updateTimestamp = args!.first as int;
        var itemType = WatchedItemTypeVm.values[args[1] as int];
        var itemId = args[2] as String;
        var itemUpdateCategory = args[3] as int;
        var title = args[4] as String;
        var details = args.last as String?;

        _serverEventChannel.add(
          Tuple2(
            ServerEventType.notification,
            Tuple2<NotificationEventType, Object>(
              NotificationEventType.newOne,
              Tuple6(
                updateTimestamp,
                itemType,
                itemId,
                itemUpdateCategory,
                title,
                details,
              ),
            ),
          ),
        );
      },
    );

    hubConnection.on(
      'OnInitialNotificationRetrieve',
      (List<Object?>? args) {
        var updates = args!.first as List<dynamic>;
        var notifications =
            updates.map((map) => NotificationVm.fromMap(map)).toList();

        _serverEventChannel.add(
          Tuple2(
            ServerEventType.notification,
            Tuple2<NotificationEventType, Object>(
              NotificationEventType.initialRetrieve,
              notifications,
            ),
          ),
        );
      },
    );

    await hubConnection.start(); // @@TODO: Handle error.

    _serverEventChannel.add(Tuple2(ServerEventType.connected, true));
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
