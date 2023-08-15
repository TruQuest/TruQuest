import 'dart:async';
import 'dart:typed_data';

import 'package:dio/dio.dart';

import '../../general/models/im/watch_command.dart';
import '../models/rvm/get_settlement_proposals_list_rvm.dart';
import '../../general/errors/vote_error.dart';
import '../models/im/cast_acceptance_poll_vote_command.dart';
import '../models/im/new_acceptance_poll_vote_im.dart';
import '../models/rvm/get_verifier_lottery_participants_rvm.dart';
import '../models/rvm/get_thing_rvm.dart';
import '../models/im/submit_new_thing_command.dart';
import '../models/rvm/get_votes_rvm.dart';
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

  ThingApiService(this._serverConnector) : _dio = _serverConnector.dio {
    _serverConnector.serverEvent$.where((event) => event.$1 == ServerEventType.thing).listen(
      (event) {
        var (thingEventType, thingId, data) = event.$2 as (ThingEventType, String, Object);
        if (thingEventType == ThingEventType.draftCreationProgress) {
          var percent = data as int;
          if (_thingIdToProgressChannel.containsKey(thingId)) {
            _thingIdToProgressChannel[thingId]!.add(percent);
            if (percent == 100) {
              _thingIdToProgressChannel.remove(thingId)!.close();
            }
          }
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
              return const ValidationError();
            } else if (error['type'] == 'File') {
              return FileError(error['errors'].values.first.first);
            } else if (error['type'] == 'Thing') {
              return ThingError(error['errors'].values.first.first);
            } else if (error['type'] == 'Vote') {
              return VoteError(error['errors'].values.first.first);
            }
            // @@TODO: Handle ServerError.
            break;
          case 401:
            var errorMessage = dioError.response!.data['error']['errors'].values.first.first;
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

  Future<Stream<int>> createNewThingDraft(
    String subjectId,
    String title,
    String details,
    String? imageExt,
    Uint8List? imageBytes,
    Uint8List? croppedImageBytes,
    List<String> evidence,
    List<TagIm> tags,
  ) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
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
        tags: tags,
      );

      var response = await _dio.post(
        '/things/draft',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
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
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.get(
        '/things/$thingId',
        options: accessToken != null
            ? Options(
                headers: {'Authorization': 'Bearer $accessToken'},
              )
            : null,
      );

      return GetThingRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future<SubmitNewThingRvm> submitNewThing(String thingId) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.post(
        '/things/submit',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
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

  Future<String> castThingAcceptancePollVote(
    NewAcceptancePollVoteIm vote,
    String signature,
  ) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.post(
        '/things/${vote.thingId}/vote',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: CastAcceptancePollVoteCommand(
          input: vote,
          signature: signature,
        ).toJson(),
      );

      return response.data['data'] as String;
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future<GetVotesRvm> getVotes(String thingId) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.get(
        '/things/$thingId/votes',
        options: accessToken != null ? Options(headers: {'Authorization': 'Bearer $accessToken'}) : null,
      );
      return GetVotesRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future<GetSettlementProposalsListRvm> getSettlementProposalsList(
    String thingId,
  ) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.get(
        '/things/$thingId/settlement-proposals',
        options: accessToken != null
            ? Options(
                headers: {'Authorization': 'Bearer $accessToken'},
              )
            : null,
      );

      return GetSettlementProposalsListRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future watch(String thingId, bool markedAsWatched) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      await _dio.post(
        '/things/watch',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
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
