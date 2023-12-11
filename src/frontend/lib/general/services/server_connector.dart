import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:logging/logging.dart';
import 'package:rxdart/rxdart.dart';
import 'package:signalr_netcore/signalr_client.dart';

import '../models/vm/notification_vm.dart';
import '../models/vm/watched_item_type_vm.dart';
import '../utils/logger.dart';

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

  late final Logger _logger;

  final Completer _initialConnectionEstablished = Completer();

  final BehaviorSubject<Future<(HubConnection, String?)>> _connectionTaskQueue =
      BehaviorSubject<Future<(HubConnection, String?)>>();
  Stream<Future<(HubConnection, String?)>> get connectionTask$ => _connectionTaskQueue.stream;

  Future<(HubConnection, String?)> get latestConnection async {
    await _initialConnectionEstablished.future;
    return _connectionTaskQueue.value;
  }

  final StreamController<(ServerEventType, Object)> _serverEventChannel =
      StreamController<(ServerEventType, Object)>.broadcast();
  Stream<(ServerEventType, Object)> get serverEvent$ => _serverEventChannel.stream;

  ServerConnector() {
    dio = Dio(BaseOptions(baseUrl: dotenv.env['ORCHESTRATOR_HOST']!));
    _logger = Logger('SignalR');
    _logger.level = Level.ALL;
    _logger.onRecord.listen((LogRecord record) => logger.info('${record.loggerName}: ${record.message}'));
  }

  Future<(HubConnection, String?)> _connectToHub(String? userId, String? token) async {
    var hubConnection = HubConnectionBuilder()
        .withUrl(
          '${dotenv.env['ORCHESTRATOR_HOST']}/api/hub',
          options: HttpConnectionOptions(
            transport: HttpTransportType.WebSockets,
            logger: _logger,
            logMessageContent: true,
            accessTokenFactory: token != null ? () => Future.value(token) : null,
          ),
        )
        .configureLogging(_logger)
        .build();

    hubConnection.keepAliveIntervalInMilliseconds = 90 * 1000;
    hubConnection.serverTimeoutInMilliseconds = 180 * 1000;

    hubConnection.onclose(
      ({error}) => error == null
          ? _logger.info('HubConnection closed')
          : _logger.warning('HubConnection closed with error: $error'),
    );

    hubConnection.on(
      'TellAboutNewThingDraftCreationProgress',
      (List<Object?>? args) {
        var thingId = args!.first as String;
        _serverEventChannel.add(
          (
            ServerEventType.thing,
            (
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
          (
            ServerEventType.settlement,
            (
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
          (
            ServerEventType.notification,
            (
              NotificationEventType.newOne,
              (
                userId,
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
        var notifications = updates.map((map) => NotificationVm.fromMap(map)).toList();

        _serverEventChannel.add(
          (
            ServerEventType.notification,
            (
              NotificationEventType.initialRetrieve,
              (
                userId!,
                notifications,
              ),
            ),
          ),
        );
      },
    );

    await hubConnection.start(); // @@TODO: Handle error.

    return (hubConnection, token);
  }

  void connectToHub(String? userId, String? accessToken) async {
    var prevConnectionTask = _connectionTaskQueue.valueOrNull;
    _connectionTaskQueue.add(() async {
      if (prevConnectionTask != null) {
        var prevConnection = await prevConnectionTask;
        try {
          await prevConnection.$1.stop();
        } catch (e) {
          _logger.warning('Error trying to stop hub connection: $e');
        }
      }

      var connection = await _connectToHub(userId, accessToken);
      if (prevConnectionTask == null) {
        _initialConnectionEstablished.complete();
      }

      return connection;
    }());
  }
}
