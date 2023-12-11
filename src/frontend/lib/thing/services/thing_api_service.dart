import 'dart:async';
import 'dart:typed_data';

import 'package:dio/dio.dart';

import '../../general/models/im/watch_command.dart';
import '../../general/utils/utils.dart';
import '../models/vm/get_settlement_proposals_list_rvm.dart';
import '../models/im/cast_validation_poll_vote_command.dart';
import '../models/im/new_thing_validation_poll_vote_im.dart';
import '../models/vm/get_verifier_lottery_participants_rvm.dart';
import '../models/vm/get_thing_rvm.dart';
import '../models/im/submit_new_thing_command.dart';
import '../models/vm/get_votes_rvm.dart';
import '../models/vm/submit_new_thing_rvm.dart';
import '../../general/models/im/tag_im.dart';
import '../models/im/thing_evidence_im.dart';
import '../../general/services/server_connector.dart';
import '../models/im/new_thing_im.dart';

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
        evidence: evidence.map((url) => ThingEvidenceIm(url: url)).toList(),
        tags: tags,
      );

      var response = await _dio.post(
        '/api/things/draft',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: input.toFormData(),
      );

      var thingId = response.data['data'] as String;

      var progressChannel = StreamController<int>.broadcast();
      _thingIdToProgressChannel[thingId] = progressChannel;

      return progressChannel.stream;
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }

  Future<GetThingRvm> getThing(String thingId) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.get(
        '/api/things/$thingId',
        options: accessToken != null
            ? Options(
                headers: {'Authorization': 'Bearer $accessToken'},
              )
            : null,
      );

      return GetThingRvm.fromMap(response.data['data']);
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }

  Future<SubmitNewThingRvm> submitNewThing(String thingId) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.post(
        '/api/things/submit',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: SubmitNewThingCommand(thingId: thingId).toJson(),
      );

      return SubmitNewThingRvm.fromMap(response.data['data']);
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }

  Future<GetVerifierLotteryParticipantsRvm> getVerifierLotteryParticipants(
    String thingId,
  ) async {
    try {
      var response = await _dio.get('/api/things/$thingId/lottery-participants');
      return GetVerifierLotteryParticipantsRvm.fromMap(response.data['data']);
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }

  Future<String> castThingValidationPollVote(
    NewThingValidationPollVoteIm vote,
    String signature,
  ) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.post(
        '/api/things/${vote.thingId}/vote',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: CastValidationPollVoteCommand(
          input: vote,
          signature: signature,
        ).toJson(),
      );

      return response.data['data'] as String;
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }

  Future<GetVotesRvm> getVotes(String thingId) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.get(
        '/api/things/$thingId/votes',
        options: accessToken != null ? Options(headers: {'Authorization': 'Bearer $accessToken'}) : null,
      );
      return GetVotesRvm.fromMap(response.data['data']);
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }

  Future<GetSettlementProposalsListRvm> getSettlementProposalsList(String thingId) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.get(
        '/api/things/$thingId/settlement-proposals',
        options: accessToken != null
            ? Options(
                headers: {'Authorization': 'Bearer $accessToken'},
              )
            : null,
      );

      return GetSettlementProposalsListRvm.fromMap(response.data['data']);
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }

  Future watch(String thingId, bool markedAsWatched) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      await _dio.post(
        '/api/things/watch',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: WatchCommand(
          thingId: thingId,
          markedAsWatched: markedAsWatched,
        ).toJson(),
      );
    } on DioException catch (ex) {
      throw wrapDioException(ex);
    }
  }
}
