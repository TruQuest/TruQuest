import 'dart:async';

import 'package:tuple/tuple.dart';

import '../models/im/evidence_im.dart';
import '../../general/errors/connection_error.dart';
import '../../general/errors/error.dart';
import '../../general/services/server_connector.dart';
import '../models/im/create_new_thing_draft_command.dart';
import '../models/im/new_thing_im.dart';
import '../../general/errors/forbidden_error.dart';
import '../../general/errors/invalid_authentication_token_error.dart';
import '../../general/errors/server_error.dart';
import '../../general/errors/validation_error.dart';
import '../errors/thing_error.dart';
import '../models/im/tag_im.dart';

class ThingApiService {
  final ServerConnector _serverConnector;

  final Map<String, StreamController<int>> _thingIdToProgressChannel = {};

  ThingApiService(this._serverConnector) {
    _serverConnector.serverEvent$
        .where((event) => event.item1 == ServerEventType.thing)
        .listen(
      (event) {
        var data = event.item2 as Tuple2<String, int>;
        var thingId = data.item1;
        var percent = data.item2;
        if (_thingIdToProgressChannel.containsKey(thingId)) {
          _thingIdToProgressChannel[thingId]!.add(percent);
          if (percent == 100) {
            _thingIdToProgressChannel.remove(thingId)!.close();
          }
        }
      },
    );
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

  Future<Stream<int>> createNewThingDraft() async {
    var hubConnection = _serverConnector.hubConnection;
    if (hubConnection == null) {
      throw ConnectionError();
    }

    try {
      var result = await hubConnection.invoke(
        'CreateNewThingDraft',
        args: [
          CreateNewThingDraftCommand(
            input: NewThingIm(
              subjectId: 'a3464bc9-2e22-457f-a65f-4a6d5bb6e6d9',
              title: 'title',
              details: 'details',
              imageUrl:
                  'https://images.newscientist.com/wp-content/uploads/2022/09/09152048/SEI_124263525.jpg',
              evidence: [
                EvidenceIm(url: 'https://stackoverflow.com/'),
                EvidenceIm(url: 'https://twitter.com/'),
              ],
              tags: [
                TagIm(id: 1),
              ],
            ),
          )
        ],
      );

      var thingId = (result as Map<String, dynamic>)['data'];

      var progressChannel = StreamController<int>.broadcast();
      _thingIdToProgressChannel[thingId] = progressChannel;

      return progressChannel.stream;
    } on Exception catch (ex) {
      throw _wrapHubException(ex);
    }
  }
}
