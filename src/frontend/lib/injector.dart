import 'package:kiwi/kiwi.dart';

import 'general/contexts/document_context.dart';
import 'thing/services/thing_api_service.dart';
import 'general/services/server_connector.dart';
import 'thing/bloc/thing_bloc.dart';
import 'thing/services/thing_service.dart';
import 'user/services/user_api_service.dart';
import 'ethereum/bloc/ethereum_bloc.dart';
import 'ethereum/services/ethereum_service.dart';
import 'user/bloc/user_bloc.dart';
import 'user/services/user_service.dart';

part 'injector.g.dart';

abstract class Injector {
  @Register.singleton(UserBloc)
  @Register.singleton(UserService)
  @Register.singleton(EthereumBloc)
  @Register.singleton(EthereumService)
  @Register.singleton(ServerConnector)
  @Register.singleton(UserApiService)
  @Register.singleton(ThingApiService)
  @Register.singleton(ThingService)
  @Register.singleton(ThingBloc)
  @Register.factory(DocumentContext)
  void configure();
}

void setup() {
  var injector = _$Injector();
  injector.configure();
}
