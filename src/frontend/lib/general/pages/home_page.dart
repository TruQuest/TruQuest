import 'package:flutter/material.dart';
import 'package:flame/game.dart';
import 'package:fluttertoast/fluttertoast.dart';

import '../widgets/clipped_rect.dart';
import '../widgets/nav_panel.dart';
import '../bloc/notification_bloc.dart';
import '../../settlement/pages/settlement_proposal_page.dart';
import '../../pong/game.dart';
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

  PageController? _pageController;

  PongGame? _game;
  BoxConstraints? _gameWidgetConstraints;

  late final FToast _fToast;

  // @@NOTE: Despite doing this, switching pages still causes these widgets to be disposed.
  late final List<Widget> _headers = [
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
    ),
    NavPanel(),
  ];

  @override
  void initState() {
    super.initState();

    _subscriptionManager.init();
    _pageContext.init().then(
          (pageController) => setState(() => _pageController = pageController),
        );

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
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Color(0xffF8F9FA),
      body: _pageController == null
          ? Center(child: CircularProgressIndicator())
          : PageView.builder(
              controller: _pageController,
              physics: NeverScrollableScrollPhysics(),
              itemBuilder: (context, index) {
                if (index == 0) {
                  return CustomScrollView(
                    slivers: [
                      ..._headers,
                      SubjectsPage(),
                    ],
                  );
                } else if (index == 1) {
                  return CustomScrollView(
                    slivers: [
                      ..._headers,
                      SliverFillRemaining(
                        hasScrollBody: false,
                        child: Center(
                          child: Text('Things'),
                        ),
                      ),
                    ],
                  );
                } else if (index == 2) {
                  return Center(
                    child: Text('How To'),
                  );
                } else if (index == 3) {
                  return LayoutBuilder(
                    builder: (context, constraints) {
                      if (_game == null ||
                          _gameWidgetConstraints != constraints) {
                        _game = PongGame();
                        _gameWidgetConstraints = constraints;
                      }
                      return GameWidget(game: _game!);
                    },
                  );
                } else if (index == 4) {
                  return CustomScrollView(
                    slivers: [
                      ..._headers,
                      SliverFillRemaining(
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
                      ),
                    ],
                  );
                }

                var route = _pageContext.currentRoute;
                var routeSplit = route.split('/');
                if (routeSplit[1] == 'subjects') {
                  var subjectId = routeSplit.last;
                  return SubjectPage(subjectId: subjectId);
                } else if (routeSplit[1] == 'things') {
                  var thingId = routeSplit.last;
                  return CustomScrollView(
                    slivers: [
                      ..._headers,
                      ThingPage(thingId: thingId),
                    ],
                  );
                } else if (routeSplit[1] == 'proposals') {
                  var proposalId = routeSplit.last;
                  return SettlementProposalPage(proposalId: proposalId);
                }

                return Center(child: Text('Not Found'));
              },
            ),
    );
  }
}
