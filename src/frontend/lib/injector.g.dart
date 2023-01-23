// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'injector.dart';

// **************************************************************************
// KiwiInjectorGenerator
// **************************************************************************

class _$Injector extends Injector {
  @override
  void configure() {
    final KiwiContainer container = KiwiContainer();
    container
      ..registerSingleton((c) => UserBloc(c<UserService>()))
      ..registerSingleton((c) => UserService(
          c<EthereumService>(), c<UserApiService>(), c<ServerConnector>()))
      ..registerSingleton((c) => EthereumBloc(c<EthereumService>()))
      ..registerSingleton((c) => EthereumService())
      ..registerSingleton((c) => ServerConnector())
      ..registerSingleton((c) => UserApiService(c<ServerConnector>()))
      ..registerSingleton((c) => ThingApiService(c<ServerConnector>()))
      ..registerSingleton((c) => ThingService(
          c<ThingApiService>(),
          c<EthereumService>(),
          c<TruQuestContract>(),
          c<ThingSubmissionVerifierLotteryContract>(),
          c<AcceptancePollContract>()))
      ..registerSingleton((c) => ThingBloc(c<ThingService>()))
      ..registerFactory((c) => DocumentContext())
      ..registerSingleton(
          (c) => NotificationBloc(c<ThingService>(), c<ThingApiService>()))
      ..registerSingleton((c) => SubjectBloc(c<SubjectService>()))
      ..registerSingleton((c) => SubjectService(c<SubjectApiService>()))
      ..registerSingleton((c) => SubjectApiService(c<ServerConnector>()))
      ..registerFactory((c) => PageContext())
      ..registerSingleton((c) => TruQuestContract(c<EthereumService>()))
      ..registerSingleton(
          (c) => ThingSubmissionVerifierLotteryContract(c<EthereumService>()))
      ..registerSingleton((c) => AcceptancePollContract(c<EthereumService>()));
  }
}
