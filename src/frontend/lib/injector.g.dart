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
      ..registerSingleton((c) => UserBloc(c<ToastMessenger>(), c<UserService>(),
          c<LocalWalletService>(), c<ThirdPartyWalletService>()))
      ..registerSingleton((c) => UserService(
          c<LocalWalletService>(),
          c<ThirdPartyWalletService>(),
          c<UserApiService>(),
          c<ServerConnector>(),
          c<LocalStorage>(),
          c<UserOperationService>(),
          c<TruQuestContract>(),
          c<TruthserumContract>()))
      ..registerSingleton(
          (c) => EthereumBloc(c<ToastMessenger>(), c<EthereumRpcProvider>()))
      ..registerSingleton((c) => ServerConnector())
      ..registerSingleton((c) => UserApiService(c<ServerConnector>()))
      ..registerSingleton((c) => ThingApiService(c<ServerConnector>()))
      ..registerSingleton((c) => ThingService(
          c<ThingApiService>(),
          c<UserService>(),
          c<UserOperationService>(),
          c<TruQuestContract>(),
          c<ThingSubmissionVerifierLotteryContract>(),
          c<AcceptancePollContract>()))
      ..registerSingleton(
          (c) => ThingBloc(c<ToastMessenger>(), c<ThingService>()))
      ..registerFactory((c) => DocumentContext())
      ..registerSingleton((c) => NotificationBloc(c<NotificationsCache>(),
          c<ToastMessenger>(), c<ThingService>(), c<SettlementService>()))
      ..registerSingleton(
          (c) => SubjectBloc(c<ToastMessenger>(), c<SubjectService>()))
      ..registerSingleton((c) => SubjectService(c<SubjectApiService>()))
      ..registerSingleton((c) => SubjectApiService(c<ServerConnector>()))
      ..registerSingleton((c) => PageContext(c<LocalStorage>()))
      ..registerSingleton((c) => TruthserumContract(c<EthereumRpcProvider>()))
      ..registerSingleton((c) => TruQuestContract(c<EthereumRpcProvider>()))
      ..registerSingleton((c) =>
          ThingSubmissionVerifierLotteryContract(c<EthereumRpcProvider>()))
      ..registerSingleton(
          (c) => AcceptancePollContract(c<EthereumRpcProvider>()))
      ..registerSingleton(
          (c) => SettlementBloc(c<ToastMessenger>(), c<SettlementService>()))
      ..registerSingleton((c) => SettlementService(
          c<SettlementApiService>(),
          c<UserService>(),
          c<UserOperationService>(),
          c<TruQuestContract>(),
          c<AcceptancePollContract>(),
          c<ThingAssessmentVerifierLotteryContract>(),
          c<AssessmentPollContract>()))
      ..registerSingleton((c) => SettlementApiService(c<ServerConnector>()))
      ..registerSingleton((c) =>
          ThingAssessmentVerifierLotteryContract(c<EthereumRpcProvider>()))
      ..registerSingleton(
          (c) => AssessmentPollContract(c<EthereumRpcProvider>()))
      ..registerSingleton((c) => NotificationsCache(
          c<UserService>(), c<UserApiService>(), c<ServerConnector>()))
      ..registerSingleton(
          (c) => SubscriptionManager(c<PageContext>(), c<ServerConnector>()))
      ..registerSingleton((c) => LocalStorage())
      ..registerSingleton((c) => ToastMessenger())
      ..registerSingleton((c) => GeneralApiService(c<ServerConnector>()))
      ..registerSingleton((c) => GeneralService(c<GeneralApiService>()))
      ..registerSingleton(
          (c) => GeneralBloc(c<ToastMessenger>(), c<GeneralService>()))
      ..registerSingleton((c) => EthereumRpcProvider())
      ..registerSingleton<IEntryPointContract>(
          (c) => EntryPointContract(c<EthereumRpcProvider>()))
      ..registerSingleton<IAccountFactoryContract>(
          (c) => SimpleAccountFactoryContract(c<EthereumRpcProvider>()))
      ..registerSingleton((c) => DummyContract(c<EthereumRpcProvider>()))
      ..registerSingleton((c) =>
          LocalWalletService(c<LocalStorage>(), c<IAccountFactoryContract>()))
      ..registerSingleton((c) => EthereumApiService(c<IEntryPointContract>()))
      ..registerSingleton((c) => UserOperationService(c<EthereumApiService>()))
      ..registerSingleton(
          (c) => ThirdPartyWalletService(c<IAccountFactoryContract>()))
      ..registerFactory((c) => UserOperationBuilder(
          c<UserService>(),
          c<EthereumApiService>(),
          c<IEntryPointContract>(),
          c<IAccountFactoryContract>()));
  }
}
