import 'dart:async';

import 'package:dio/dio.dart';
import 'package:logging/logging.dart';
import 'package:rxdart/rxdart.dart';
import 'package:signalr_netcore/signalr_client.dart';
import 'package:tuple/tuple.dart';

import '../models/rvm/notification_vm.dart';
import '../models/rvm/watched_item_type_vm.dart';

enum ServerEventType {
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

  final Completer _initialConnectionEstablished = Completer();

  final BehaviorSubject<Future<Tuple2<HubConnection, String?>>>
      _connectionTaskQueue =
      BehaviorSubject<Future<Tuple2<HubConnection, String?>>>();
  Stream<Future<Tuple2<HubConnection, String?>>> get connectionTask$ =>
      _connectionTaskQueue.stream;

  Future<Tuple2<HubConnection, String?>> get latestConnection async {
    await _initialConnectionEstablished.future;
    return _connectionTaskQueue.value;
  }

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
  }

  Future<Tuple2<HubConnection, String?>> _connectToHub(
    String? username,
    String? token,
  ) async {
    var hubLogger = Logger('Hub');
    var transportLogger = Logger('Transport');

    var hubConnection = HubConnectionBuilder()
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
              Tuple7(
                username,
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
              Tuple2(
                username!,
                notifications,
              ),
            ),
          ),
        );
      },
    );

    await hubConnection.start(); // @@TODO: Handle error.

    return Tuple2(hubConnection, token);
  }

  void connectToHub(String? username, String? accessToken) async {
    var prevConnectionTask = _connectionTaskQueue.valueOrNull;
    _connectionTaskQueue.add(() async {
      if (prevConnectionTask != null) {
        var prevConnection = await prevConnectionTask;
        try {
          await prevConnection.item1.stop();
        } catch (e) {
          print(e);
        }
      }

      var connection = await _connectToHub(username, accessToken);
      if (prevConnectionTask == null) {
        _initialConnectionEstablished.complete();
      }

      return connection;
    }());
  }
}
