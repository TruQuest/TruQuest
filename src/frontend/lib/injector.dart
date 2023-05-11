import 'package:kiwi/kiwi.dart';

import 'general/services/notifications_cache.dart';
import 'general/contracts/assessment_poll_contract.dart';
import 'general/contracts/thing_assessment_verifier_lottery_contract.dart';
import 'settlement/bloc/settlement_bloc.dart';
import 'settlement/services/settlement_api_service.dart';
import 'settlement/services/settlement_service.dart';
import 'general/contracts/acceptance_poll_contract.dart';
import 'general/contracts/thing_submission_verifier_lottery_contract.dart';
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
  @Register.singleton(NotificationBloc)
  @Register.singleton(SubjectBloc)
  @Register.singleton(SubjectService)
  @Register.singleton(SubjectApiService)
  @Register.factory(PageContext)
  @Register.singleton(TruQuestContract)
  @Register.singleton(ThingSubmissionVerifierLotteryContract)
  @Register.singleton(AcceptancePollContract)
  @Register.singleton(SettlementBloc)
  @Register.singleton(SettlementService)
  @Register.singleton(SettlementApiService)
  @Register.singleton(ThingAssessmentVerifierLotteryContract)
  @Register.singleton(AssessmentPollContract)
  @Register.singleton(NotificationsCache)
  void configure();
}

void setup() {
  var injector = _$Injector();
  injector.configure();
}
