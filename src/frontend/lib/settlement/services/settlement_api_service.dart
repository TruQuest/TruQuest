import 'dart:async';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:tuple/tuple.dart';

import '../models/rvm/get_settlement_proposals_rvm.dart';
import '../models/im/new_settlement_proposal_im.dart';
import '../models/im/submit_new_settlement_proposal_command.dart';
import '../models/im/verdict_im.dart';
import '../../general/errors/api_error.dart';
import '../../general/errors/error.dart';
import '../../general/errors/connection_error.dart';
import '../../general/errors/file_error.dart';
import '../../general/errors/forbidden_error.dart';
import '../../general/errors/invalid_authentication_token_error.dart';
import '../../general/errors/server_error.dart';
import '../../general/errors/validation_error.dart';
import '../../general/errors/vote_error.dart';
import '../../general/services/server_connector.dart';
import '../errors/settlement_error.dart';
import '../models/im/supporting_evidence_im.dart';
import '../models/rvm/settlement_proposal_state_vm.dart';
import '../models/rvm/submit_new_settlement_proposal_rvm.dart';

class SettlementApiService {
  final ServerConnector _serverConnector;
  final Dio _dio;

  final Map<String, StreamController<int>> _proposalIdToProgressChannel = {};

  final StreamController<Tuple3<SettlementEventType, String, Object?>>
      _proposalEventChannel =
      StreamController<Tuple3<SettlementEventType, String, Object?>>();
  Stream<Tuple3<SettlementEventType, String, Object?>> get proposalEvent$ =>
      _proposalEventChannel.stream;

  SettlementApiService(this._serverConnector) : _dio = _serverConnector.dio {
    _serverConnector.serverEvent$
        .where((event) => event.item1 == ServerEventType.settlement)
        .listen(
      (event) {
        var data = event.item2 as Tuple3<SettlementEventType, String, Object?>;
        var settlementEventType = data.item1;
        var proposalId = data.item2;
        if (settlementEventType == SettlementEventType.draftCreateProgress) {
          var percent = data.item3 as int;
          if (_proposalIdToProgressChannel.containsKey(proposalId)) {
            _proposalIdToProgressChannel[proposalId]!.add(percent);
            if (percent == 100) {
              _proposalIdToProgressChannel.remove(proposalId)!.close();
              _proposalEventChannel.add(
                Tuple3(SettlementEventType.draftCreated, proposalId, null),
              );
            }
          }
        } else if (settlementEventType == SettlementEventType.stateChanged) {
          var state = data.item3 as SettlementProposalStateVm;
          _proposalEventChannel.add(
            Tuple3(settlementEventType, proposalId, state),
          );
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
            } else if (error['type'] == 'Settlement') {
              return SettlementError(error['errors'].values.first.first);
            } else if (error['type'] == 'Vote') {
              return VoteError(error['errors'].values.first.first);
            }
            break; // @@NOTE: Should never actually reach here.
          case 401:
            var errorMessage =
                dioError.response!.data['error']['errors'].values.first.first;
            return InvalidAuthenticationTokenError(errorMessage);
          case 403:
            return ForbiddenError();
        }
    }

    print(dioError);

    return ApiError();
  }

  Future<GetSettlementProposalsRvm> getSettlementProposalsFor(
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

      return GetSettlementProposalsRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw _wrapError(error);
    }
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
        evidence:
            evidence.map((url) => SupportingEvidenceIm(url: url)).toList(),
      );

      var response = await _dio.post(
        '/proposals/draft',
        options: Options(
          headers: {'Authorization': 'Bearer ${_serverConnector.accessToken}'},
        ),
        data: input.toFormData(),
      );

      var proposalId = response.data['data'] as String;

      var progressChannel = StreamController<int>.broadcast();
      _proposalIdToProgressChannel[proposalId] = progressChannel;

      return progressChannel.stream;
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }

  Future<SubmitNewSettlementProposalRvm> submitNewSettlementProposal(
    String proposalId,
  ) async {
    try {
      var response = await _dio.post(
        '/proposals/submit',
        options: Options(
          headers: {'Authorization': 'Bearer ${_serverConnector.accessToken}'},
        ),
        data: SubmitNewSettlementProposalCommand(
          proposalId: proposalId,
        ).toJson(),
      );

      return SubmitNewSettlementProposalRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw _wrapError(error);
    }
  }
}
