import 'package:flutter/material.dart';
import 'package:bot_toast/bot_toast.dart';

import '../widgets/fixed_width.dart';
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

  @override
  void initState() {
    super.initState();

    _subscriptionManager.init();
    _pageContext.init();

    _notificationBloc.toast$.listen(
      (message) => BotToast.showSimpleNotification(
        title: message,
        duration: const Duration(seconds: 5),
      ),
    );
  }

  @override
  Widget buildX(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xffF8F9FA),
      body: FixedWidth(
        width: 1400,
        child: CustomScrollView(
          slivers: [
            SliverAppBar(
              floating: true,
              pinned: false,
              snap: false,
              backgroundColor: const Color(0xffF8F9FA),
              leadingWidth: 120,
              leading: const ClippedRect(
                height: 70,
                color: Colors.black,
                fromNarrowToWide: true,
              ),
              title: Image.asset(
                'assets/images/logo.png',
                height: 60,
              ),
              toolbarHeight: 70,
              centerTitle: true,
              elevation: 0,
              actions: [
                Banner(
                  message: 'pre-alpha',
                  location: BannerLocation.topEnd,
                  color: const Color.fromARGB(255, 208, 53, 76),
                ),
              ],
            ),
            NavPanel(),
            StreamBuilder(
              stream: _pageContext.route$,
              builder: (context, snapshot) {
                if (snapshot.data == null) {
                  return const SliverFillRemaining(
                    hasScrollBody: false,
                    child: Center(child: CircularProgressIndicator()),
                  );
                }

                var route = snapshot.data!;
                switch (route) {
                  case '/subjects':
                    return const SubjectsPage();
                  case '/goto':
                    return SliverFillRemaining(
                      hasScrollBody: false,
                      child: Center(
                        child: TextButton(
                          child: const Text('Go To'),
                          onPressed: () async {
                            var route = '';
                            var shouldGo = await showDialog<bool>(
                              context: context,
                              builder: (context) => AlertDialog(
                                title: const Text('Page'),
                                content: SizedBox(
                                  width: 100,
                                  child: TextField(
                                    onChanged: (value) => route = value,
                                  ),
                                ),
                                actions: [
                                  TextButton(
                                    child: const Text('Ok'),
                                    onPressed: () => Navigator.of(context).pop(true),
                                  ),
                                ],
                              ),
                            );

                            if (shouldGo != null && shouldGo && route.isNotEmpty) {
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

                    return const SliverFillRemaining(
                      hasScrollBody: false,
                      child: Center(child: Text('Not found')),
                    );
                }
              },
            ),
          ],
        ),
      ),
    );
  }
}
