import 'dart:async';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:tuple/tuple.dart';

import '../../general/models/im/watch_command.dart';
import '../models/rvm/get_settlement_proposals_list_rvm.dart';
import '../models/rvm/get_verifiers_rvm.dart';
import '../../general/errors/vote_error.dart';
import '../models/im/cast_acceptance_poll_vote_command.dart';
import '../models/im/decision_im.dart';
import '../models/im/new_acceptance_poll_vote_im.dart';
import '../models/im/unsubscribe_from_updates_command.dart';
import '../models/rvm/thing_state_vm.dart';
import '../models/im/subscribe_to_updates_command.dart';
import '../models/rvm/get_verifier_lottery_participants_rvm.dart';
import '../models/rvm/get_thing_rvm.dart';
import '../models/im/submit_new_thing_command.dart';
import '../models/rvm/submit_new_thing_rvm.dart';
import '../../general/errors/api_error.dart';
import '../../general/errors/file_error.dart';
import '../../general/models/im/tag_im.dart';
import '../models/im/evidence_im.dart';
import '../../general/errors/connection_error.dart';
import '../../general/errors/error.dart';
import '../../general/services/server_connector.dart';
import '../models/im/new_thing_im.dart';
import '../../general/errors/forbidden_error.dart';
import '../../general/errors/invalid_authentication_token_error.dart';
import '../../general/errors/server_error.dart';
import '../../general/errors/validation_error.dart';
import '../errors/thing_error.dart';

class ThingApiService {
  final ServerConnector _serverConnector;
  final Dio _dio;

  final Map<String, StreamController<int>> _thingIdToProgressChannel = {};

  final StreamController<Tuple3<ThingEventType, String, Object?>>
      _thingEventChannel =
      StreamController<Tuple3<ThingEventType, String, Object?>>();
  Stream<Tuple3<ThingEventType, String, Object?>> get thingEvent$ =>
      _thingEventChannel.stream;

  ThingApiService(this._serverConnector) : _dio = _serverConnector.dio {
    _serverConnector.serverEvent$
        .where((event) => event.item1 == ServerEventType.thing)
        .listen(
      (event) {
        var data = event.item2 as Tuple3<ThingEventType, String, Object>;
        var thingEventType = data.item1;
        var thingId = data.item2;
        if (thingEventType == ThingEventType.draftCreationProgress) {
          var percent = data.item3 as int;
          if (_thingIdToProgressChannel.containsKey(thingId)) {
            _thingIdToProgressChannel[thingId]!.add(percent);
            if (percent == 100) {
              _thingIdToProgressChannel.remove(thingId)!.close();
              _thingEventChannel.add(
                Tuple3(ThingEventType.draftCreated, thingId, null),
              );
            }
          }
        } else if (thingEventType == ThingEventType.stateChanged) {
          var state = data.item3 as ThingStateVm;
          _thingEventChannel.add(Tuple3(thingEventType, thingId, state));
        }
      },
    );
  }

  Error _wrapError(DioError dioError) {
    switch (dioError.type) {
      case DioErrorType.connectTimeout:
      case DioErrorType.sendTimeout:
      case DioErrorType.receiveTimeout:
        return ConnectionError();
      case DioErrorType.response:
        var statusCode = dioError.response!.statusCode!;
        if (statusCode >= 500) {
          return ServerError();
        }

        switch (statusCode) {
          case 400:
            var error = dioError.response!.data['error'];
            if (error['type'] == 'Validation') {
              return ValidationError();
            } else if (error['type'] == 'File') {
              return FileError(error['errors'].values.first.first);
            } else if (error['type'] == 'Thing') {
              return ThingError(error['errors'].values.first.first);
            } else if (error['type'] == 'Vote') {
              return VoteError(error['errors'].values.first.first);
            }
            break;
          case 401:
            var errorMessage =
                dioError.response!.data['error']['errors'].values.first.first;
            return InvalidAuthenticationTokenError(errorMessage);
          case 403:
            return ForbiddenError();
        }

        throw UnimplementedError();
      default:
        print(dioError);
        return ApiError();
    }
  }

  Error _wrapHubException(Exception ex) {
    var errorMessage = ex.toString();
    if (errorMessage.contains('[AuthorizationError]')) {
      if (errorMessage.contains('Forbidden')) {
        return ForbiddenError();
        // } else if (errorMessage.contains('token expired at')) {
        //   return AuthenticationTokenExpiredError();
      } else {
        return InvalidAuthenticationTokenError(
          errorMessage.split('[AuthorizationError] ').last,
        );
      }
    } else if (errorMessage.contains('[ValidationError]')) {
      return ValidationError();
    } else if (errorMessage.contains('[ThingError]')) {
      return ThingError(errorMessage.split('[ThingError] ').last);
    }

    print(ex);

    return ServerError();
  }

  Future<Stream<int>> createNewThingDraft(
    String subjectId,
    String title,
    String details,
    String? imageExt,
    Uint8List? imageBytes,
    Uint8List? croppedImageBytes,
    List<String> evidence,
    List<int> tags,
  ) async {
    try {
      var input = NewThingIm(
        subjectId: subjectId,
        title: title,
        details: details,
        imageExt: imageExt,
        imageBytes: imageBytes,
        croppedImageExt: croppedImageBytes != null ? 'png' : null,
        croppedImageBytes: croppedImageBytes,
        evidence: evidence.map((url) => EvidenceIm(url: url)).toList(),
        tags: tags.map((tagId) => TagIm(id: tagId)).toList(),
      );

      var response = await _dio.post(
        '/things/draft',
        options: Options(
          headers: {'Authorization': 'Bearer ${_serverConnector.accessToken}'},
        ),
        data: input.toFormData(),
      );

      var thingId = response.data['data'] as String;

      var progressChannel = StreamController<int>.broadcast();
      _thingIdToProgressChannel[thingId] = progressChannel;

      return progressChannel.stream;
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future<GetThingRvm> getThing(String thingId) async {
    try {
      var response = await _dio.get(
        '/things/$thingId',
        options: _serverConnector.accessToken != null
            ? Options(
                headers: {
                  'Authorization': 'Bearer ${_serverConnector.accessToken}'
                },
              )
            : null,
      );

      return GetThingRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future subscribeToThing(String thingId) async {
    var hubConnection = _serverConnector.hubConnection;
    if (hubConnection == null) {
      print('Not connected to hub!');
      return;
    }

    try {
      await hubConnection.invoke(
        'SubscribeToThingUpdates',
        args: [
          SubscribeToUpdatesCommand(thingId: thingId),
        ],
      );
    } on Exception catch (ex) {
      throw _wrapHubException(ex);
    }
  }

  Future<SubmitNewThingRvm> submitNewThing(String thingId) async {
    try {
      var response = await _dio.post(
        '/things/submit',
        options: Options(
          headers: {'Authorization': 'Bearer ${_serverConnector.accessToken}'},
        ),
        data: SubmitNewThingCommand(thingId: thingId).toJson(),
      );

      return SubmitNewThingRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future<GetVerifierLotteryParticipantsRvm> getVerifierLotteryParticipants(
    String thingId,
  ) async {
    try {
      var response = await _dio.get('/things/$thingId/lottery-participants');
      return GetVerifierLotteryParticipantsRvm.fromMap(
        response.data['data'],
      );
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future unsubscribeFromThing(String thingId) async {
    var hubConnection = _serverConnector.hubConnection;
    if (hubConnection == null) {
      print('Not connected to hub!');
      return;
    }

    try {
      await hubConnection.invoke(
        'UnsubscribeFromThingUpdates',
        args: [
          UnsubscribeFromUpdatesCommand(thingId: thingId),
        ],
      );
    } on Exception catch (ex) {
      throw _wrapHubException(ex);
    }
  }

  Future<String> castThingAcceptancePollVote(
    String thingId,
    String castedAt,
    DecisionIm decision,
    String reason,
    String signature,
  ) async {
    try {
      var response = await _dio.post(
        '/things/$thingId/vote',
        options: Options(
          headers: {'Authorization': 'Bearer ${_serverConnector.accessToken}'},
        ),
        data: CastAcceptancePollVoteCommand(
          input: NewAcceptancePollVoteIm(
            castedAt: castedAt,
            decision: decision,
            reason: reason,
          ),
          signature: signature,
        ).toJson(),
      );

      return response.data['data'] as String;
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future<GetVerifiersRvm> getVerifiers(String thingId) async {
    try {
      var response = await _dio.get('/things/$thingId/verifiers');
      return GetVerifiersRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future<GetSettlementProposalsListRvm> getSettlementProposalsList(
    String thingId,
  ) async {
    try {
      var response = await _dio.get(
        '/things/$thingId/settlement-proposals',
        options: _serverConnector.accessToken != null
            ? Options(
                headers: {
                  'Authorization': 'Bearer ${_serverConnector.accessToken}'
                },
              )
            : null,
      );

      return GetSettlementProposalsListRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future watch(String thingId, bool markedAsWatched) async {
    try {
      await _dio.post(
        '/things/watch',
        options: Options(
          headers: {'Authorization': 'Bearer ${_serverConnector.accessToken}'},
        ),
        data: WatchCommand(
          thingId: thingId,
          markedAsWatched: markedAsWatched,
        ).toJson(),
      );
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }
}
