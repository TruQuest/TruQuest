import 'package:flutter/material.dart';
import 'package:fluttertoast/fluttertoast.dart';

import '../widgets/deposit_funds_button.dart';
import '../widgets/clipped_rect.dart';
import '../widgets/nav_panel.dart';
import '../bloc/notification_bloc.dart';
import '../../settlement/pages/settlement_proposal_page.dart';
import '../../subject/pages/subject_page.dart';
import '../../thing/pages/thing_page.dart';
import '../contexts/page_context.dart';
import '../../subject/pages/subjects_page.dart';
import '../../widget_extensions.dart';
import '../services/subscription_manager.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends StateX<HomePage> {
  late final _subscriptionManager = use<SubscriptionManager>();
  late final _pageContext = use<PageContext>();
  late final _notificationBloc = use<NotificationBloc>();

  late final FToast _fToast;

  @override
  void initState() {
    super.initState();

    _subscriptionManager.init();
    _pageContext.init();

    _fToast = FToast();
    _fToast.init(context);
    _notificationBloc.toast$.listen(
      (toast) => _fToast.showToast(
        child: toast,
        gravity: ToastGravity.BOTTOM_RIGHT,
        toastDuration: Duration(seconds: 10),
      ),
    );
  }

  @override
  Widget buildX(BuildContext context) {
    return Scaffold(
      backgroundColor: Color(0xffF8F9FA),
      body: CustomScrollView(
        slivers: [
          SliverAppBar(
            floating: true,
            pinned: false,
            snap: false,
            backgroundColor: Colors.white,
            leadingWidth: 150,
            leading: ClippedRect(
              height: 70,
              color: Colors.black,
              fromNarrowToWide: true,
            ),
            title: Image.asset(
              'assets/images/logo1.png',
              height: 60,
            ),
            toolbarHeight: 70,
            centerTitle: true,
            elevation: 0,
            actions: [DepositFundsButton()],
          ),
          NavPanel(),
          StreamBuilder(
            stream: _pageContext.route$,
            builder: (context, snapshot) {
              if (snapshot.data == null) {
                return SliverFillRemaining(
                  hasScrollBody: false,
                  child: Center(child: CircularProgressIndicator()),
                );
              }

              var route = snapshot.data!;
              switch (route) {
                case '/subjects':
                  return SubjectsPage();
                case '/goto':
                  return SliverFillRemaining(
                    hasScrollBody: false,
                    child: Center(
                      child: TextButton(
                        child: Text('Go To'),
                        onPressed: () async {
                          var route = '';
                          var shouldGo = await showDialog<bool>(
                            context: context,
                            builder: (context) => AlertDialog(
                              title: Text('Page'),
                              content: SizedBox(
                                width: 100,
                                child: TextField(
                                  onChanged: (value) => route = value,
                                ),
                              ),
                              actions: [
                                TextButton(
                                  child: Text('Ok'),
                                  onPressed: () {
                                    Navigator.of(context).pop(true);
                                  },
                                ),
                              ],
                            ),
                          );

                          if (shouldGo != null &&
                              shouldGo &&
                              route.isNotEmpty) {
                            _pageContext.goto(route);
                          }
                        },
                      ),
                    ),
                  );
                default:
                  var routeSplit = route.split('/');
                  var id = routeSplit[2];
                  var timestamp = DateTime.fromMillisecondsSinceEpoch(
                    int.parse(routeSplit.last),
                  );

                  if (routeSplit[1] == 'subjects') {
                    return SubjectPage(
                      subjectId: id,
                      timestamp: timestamp,
                    );
                  } else if (routeSplit[1] == 'things') {
                    return ThingPage(
                      thingId: id,
                      timestamp: timestamp,
                    );
                  } else if (routeSplit[1] == 'proposals') {
                    return SettlementProposalPage(
                      proposalId: id,
                      timestamp: timestamp,
                    );
                  }

                  return SliverFillRemaining(
                    hasScrollBody: false,
                    child: Center(child: Text('Not found')),
                  );
              }
            },
          ),
        ],
      ),
    );
  }
}
