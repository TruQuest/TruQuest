import 'dart:async';
import 'dart:typed_data';

import 'package:dio/dio.dart';

import '../../general/utils/utils.dart';
import '../models/im/cast_assessment_poll_vote_command.dart';
import '../models/im/new_settlement_proposal_assessment_poll_vote_im.dart';
import '../models/vm/get_verifier_lottery_participants_rvm.dart';
import '../models/vm/get_settlement_proposal_rvm.dart';
import '../models/im/new_settlement_proposal_im.dart';
import '../models/im/submit_new_settlement_proposal_command.dart';
import '../models/im/verdict_im.dart';
import '../../general/services/server_connector.dart';
import '../models/im/settlement_proposal_evidence_im.dart';
import '../models/vm/get_votes_rvm.dart';
import '../models/vm/submit_new_settlement_proposal_rvm.dart';

class SettlementApiService {
  final ServerConnector _serverConnector;
  final Dio _dio;

  final Map<String, StreamController<int>> _proposalIdToProgressChannel = {};

  SettlementApiService(this._serverConnector) : _dio = _serverConnector.dio {
    _serverConnector.serverEvent$.where((event) => event.$1 == ServerEventType.settlement).listen(
      (event) {
        var (settlementEventType, proposalId, data) = event.$2 as (SettlementEventType, String, Object);
        if (settlementEventType == SettlementEventType.draftCreationProgress) {
          var percent = data as int;
          if (_proposalIdToProgressChannel.containsKey(proposalId)) {
            _proposalIdToProgressChannel[proposalId]!.add(percent);
            if (percent == 100) {
              _proposalIdToProgressChannel.remove(proposalId)!.close();
            }
          }
        }
      },
    );
  }

  Future<Stream<int>> createNewSettlementProposalDraft(
    String thingId,
    String title,
    VerdictIm verdict,
    String details,
    String? imageExt,
    Uint8List? imageBytes,
    Uint8List? croppedImageBytes,
    List<String> evidence,
  ) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var input = NewSettlementProposalIm(
        thingId: thingId,
        title: title,
        verdict: verdict,
        details: details,
        imageExt: imageExt,
        imageBytes: imageBytes,
        croppedImageExt: croppedImageBytes != null ? 'png' : null,
        croppedImageBytes: croppedImageBytes,
        evidence: evidence.map((url) => SettlementProposalEvidenceIm(url: url)).toList(),
      );

      var response = await _dio.post(
        '/proposals/draft',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: input.toFormData(),
      );

      var proposalId = response.data['data'] as String;

      var progressChannel = StreamController<int>.broadcast();
      _proposalIdToProgressChannel[proposalId] = progressChannel;

      return progressChannel.stream;
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }

  Future<GetSettlementProposalRvm> getSettlementProposal(String proposalId) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.get(
        '/proposals/$proposalId',
        options: accessToken != null
            ? Options(
                headers: {'Authorization': 'Bearer $accessToken'},
              )
            : null,
      );

      return GetSettlementProposalRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }

  Future<SubmitNewSettlementProposalRvm> submitNewSettlementProposal(String proposalId) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.post(
        '/proposals/submit',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: SubmitNewSettlementProposalCommand(
          proposalId: proposalId,
        ).toJson(),
      );

      return SubmitNewSettlementProposalRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }

  Future<GetVerifierLotteryParticipantsRvm> getVerifierLotteryParticipants(
    String thingId,
    String proposalId,
  ) async {
    try {
      var response = await _dio.get(
        '/things/$thingId/proposals/$proposalId/lottery-participants',
      );

      return GetVerifierLotteryParticipantsRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }

  Future<String> castSettlementProposalAssessmentPollVote(
    NewSettlementProposalAssessmentPollVoteIm vote,
    String signature,
  ) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.post(
        '/proposals/${vote.proposalId}/vote',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: CastAssessmentPollVoteCommand(
          input: vote,
          signature: signature,
        ).toJson(),
      );

      return response.data['data'] as String;
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }

  Future<GetVotesRvm> getVotes(String proposalId) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.get(
        '/proposals/$proposalId/votes',
        options: accessToken != null ? Options(headers: {'Authorization': 'Bearer $accessToken'}) : null,
      );
      return GetVotesRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }
}
