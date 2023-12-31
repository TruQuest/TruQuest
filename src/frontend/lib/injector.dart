import 'package:kiwi/kiwi.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';

import 'admin/bloc/admin_bloc.dart';
import 'admin/services/admin_api_service.dart';
import 'admin/services/admin_service.dart';
import 'ethereum/models/im/user_operation.dart';
import 'ethereum/services/embedded_wallet_service.dart';
import 'ethereum/services/third_party_wallet_service.dart';
import 'ethereum/services/user_operation_service.dart';
import 'ethereum/services/ethereum_rpc_provider.dart';
import 'general/contracts/erc4337/entrypoint_contract.dart';
import 'ethereum/services/ethereum_api_service.dart';
import 'general/contracts/erc4337/entrypoint_v0_6_0_contract.dart';
import 'general/contracts/erc4337/iaccount_factory_contract.dart';
import 'general/contracts/erc4337/ientrypoint_contract.dart';
import 'general/contracts/erc4337/light_account_factory_contract.dart';
import 'general/contracts/erc4337/simple_account_factory_contract.dart';
import 'general/contracts/truthserum_contract.dart';
import 'general/services/general_api_service.dart';
import 'general/bloc/general_bloc.dart';
import 'general/services/general_service.dart';
import 'general/services/iframe_manager.dart';
import 'general/services/toast_messenger.dart';
import 'general/services/local_storage.dart';
import 'general/services/subscription_manager.dart';
import 'general/services/notifications_cache.dart';
import 'general/contracts/settlement_proposal_assessment_poll_contract.dart';
import 'general/contracts/settlement_proposal_assessment_verifier_lottery_contract.dart';
import 'settlement/bloc/settlement_bloc.dart';
import 'settlement/services/settlement_api_service.dart';
import 'settlement/services/settlement_service.dart';
import 'general/contracts/thing_validation_poll_contract.dart';
import 'general/contracts/thing_validation_verifier_lottery_contract.dart';
import 'general/contracts/truquest_contract.dart';
import 'general/contexts/page_context.dart';
import 'subject/bloc/subject_bloc.dart';
import 'subject/services/subject_api_service.dart';
import 'subject/services/subject_service.dart';
import 'general/bloc/notification_bloc.dart';
import 'general/contexts/document_context.dart';
import 'thing/services/thing_api_service.dart';
import 'general/services/server_connector.dart';
import 'thing/bloc/thing_bloc.dart';
import 'thing/services/thing_service.dart';
import 'user/services/user_api_service.dart';
import 'ethereum/bloc/ethereum_bloc.dart';
import 'user/bloc/user_bloc.dart';
import 'user/services/user_service.dart';

part 'injector.g.dart';

abstract class Injector {
  @Register.singleton(UserBloc)
  @Register.singleton(UserService)
  @Register.singleton(EthereumBloc)
  @Register.singleton(ServerConnector)
  @Register.singleton(UserApiService)
  @Register.singleton(ThingApiService)
  @Register.singleton(ThingService)
  @Register.singleton(ThingBloc)
  @Register.factory(DocumentContext)
  @Register.singleton(NotificationBloc)
  @Register.singleton(SubjectBloc)
  @Register.singleton(SubjectService)
  @Register.singleton(SubjectApiService)
  @Register.singleton(PageContext)
  @Register.singleton(TruthserumContract)
  @Register.singleton(TruQuestContract)
  @Register.singleton(ThingValidationVerifierLotteryContract)
  @Register.singleton(ThingValidationPollContract)
  @Register.singleton(SettlementBloc)
  @Register.singleton(SettlementService)
  @Register.singleton(SettlementApiService)
  @Register.singleton(SettlementProposalAssessmentVerifierLotteryContract)
  @Register.singleton(SettlementProposalAssessmentPollContract)
  @Register.singleton(NotificationsCache)
  @Register.singleton(SubscriptionManager)
  @Register.singleton(LocalStorage)
  @Register.singleton(ToastMessenger)
  @Register.singleton(GeneralApiService)
  @Register.singleton(GeneralService)
  @Register.singleton(GeneralBloc)
  @Register.singleton(EthereumRpcProvider)
  @Register.singleton(EthereumApiService)
  @Register.singleton(UserOperationService)
  @Register.singleton(ThirdPartyWalletService)
  @Register.factory(UserOperationBuilder)
  @Register.singleton(IFrameManager)
  @Register.singleton(EmbeddedWalletService)
  @Register.singleton(AdminBloc)
  @Register.singleton(AdminService)
  @Register.singleton(AdminApiService)
  void configure();

  @Register.singleton(
    IEntryPointContract,
    from: EntryPointContract,
  )
  @Register.singleton(
    IAccountFactoryContract,
    from: SimpleAccountFactoryContract,
  )
  void configureDevelopment();

  @Register.singleton(
    IEntryPointContract,
    from: EntryPointV060Contract,
  )
  @Register.singleton(
    IAccountFactoryContract,
    from: LightAccountFactoryContract,
  )
  void configureStaging();
}

void setup() {
  var injector = _$Injector();
  injector.configure();

  var environment = dotenv.env['ENVIRONMENT']!;
  if (environment == 'Development' || dotenv.env['USING_SIMULATED_BLOCKCHAIN'] == '1') {
    injector.configureDevelopment();
  } else {
    injector.configureStaging();
  }
}
