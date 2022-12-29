import "package:kiwi/kiwi.dart";

import 'general/services/server_connector.dart';
import 'user/services/user_api_service.dart';
import "ethereum/bloc/ethereum_bloc.dart";
import "ethereum/services/ethereum_service.dart";
import "user/bloc/user_bloc.dart";
import "user/services/user_service.dart";

part "injector.g.dart";

abstract class Injector {
  @Register.singleton(UserBloc)
  @Register.singleton(UserService)
  @Register.singleton(EthereumBloc)
  @Register.singleton(EthereumService)
  @Register.singleton(ServerConnector)
  @Register.singleton(UserApiService)
  void configure();
}

void setup() {
  var injector = _$Injector();
  injector.configure();
}
