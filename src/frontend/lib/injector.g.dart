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
      ..registerSingleton((c) => UserBloc(
          c<ToastMessenger>(), c<UserService>(), c<EmbeddedWalletService>()))
      ..registerSingleton((c) => UserService(
          c<EmbeddedWalletService>(),
          c<ServerConnector>(),
          c<LocalStorage>(),
          c<UserOperationService>(),
          c<TruQuestContract>(),
          c<TruthserumContract>(),
          c<EthereumApiService>()))
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
          c<ThingValidationVerifierLotteryContract>(),
          c<ThingValidationPollContract>()))
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
          ThingValidationVerifierLotteryContract(c<EthereumRpcProvider>()))
      ..registerSingleton(
          (c) => ThingValidationPollContract(c<EthereumRpcProvider>()))
      ..registerSingleton(
          (c) => SettlementBloc(c<ToastMessenger>(), c<SettlementService>()))
      ..registerSingleton((c) => SettlementService(
          c<SettlementApiService>(),
          c<UserService>(),
          c<UserOperationService>(),
          c<TruQuestContract>(),
          c<ThingValidationPollContract>(),
          c<SettlementProposalAssessmentVerifierLotteryContract>(),
          c<SettlementProposalAssessmentPollContract>()))
      ..registerSingleton((c) => SettlementApiService(c<ServerConnector>()))
      ..registerSingleton((c) =>
          SettlementProposalAssessmentVerifierLotteryContract(
              c<EthereumRpcProvider>()))
      ..registerSingleton((c) =>
          SettlementProposalAssessmentPollContract(c<EthereumRpcProvider>()))
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
      ..registerSingleton((c) => EthereumApiService(c<IEntryPointContract>()))
      ..registerSingleton((c) => UserOperationService(c<EthereumRpcProvider>(),
          c<EthereumApiService>(), c<IEntryPointContract>()))
      ..registerFactory((c) => UserOperationBuilder(
          c<UserService>(),
          c<EthereumApiService>(),
          c<IEntryPointContract>(),
          c<IAccountFactoryContract>()))
      ..registerSingleton((c) => IFrameManager())
      ..registerSingleton((c) => EmbeddedWalletService(
          c<UserApiService>(), c<IFrameManager>(), c<LocalStorage>()));
  }

  @override
  void configureDevelopment() {
    final KiwiContainer container = KiwiContainer();
    container
      ..registerSingleton<IEntryPointContract>(
          (c) => EntryPointContract(c<EthereumRpcProvider>()))
      ..registerSingleton<IAccountFactoryContract>(
          (c) => SimpleAccountFactoryContract(c<EthereumRpcProvider>()));
  }

  @override
  void configureStaging() {
    final KiwiContainer container = KiwiContainer();
    container
      ..registerSingleton<IEntryPointContract>(
          (c) => EntryPointV060Contract(c<EthereumRpcProvider>()))
      ..registerSingleton<IAccountFactoryContract>(
          (c) => LightAccountFactoryContract(c<EthereumRpcProvider>()));
  }
}
