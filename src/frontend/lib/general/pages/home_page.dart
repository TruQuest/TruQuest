import 'package:flutter/material.dart';
import 'package:flame/game.dart';
import 'package:easy_sidemenu/easy_sidemenu.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

import '../../settlement/pages/settlement_proposal_page.dart';
import '../../pong/game.dart';
import '../../subject/pages/subject_page.dart';
import '../../thing/pages/thing_page.dart';
import '../contexts/page_context.dart';
import '../../subject/pages/subjects_page.dart';
import '../bloc/notification_bloc.dart';
import '../../widget_extensions.dart';
import '../services/subscription_manager.dart';
import '../widgets/status_panel.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends StateX<HomePage> {
  late final _notificationBloc = use<NotificationBloc>();
  late final _subscriptionManager = use<SubscriptionManager>();
  late final _pageContext = use<PageContext>();

  PageController? _pageController;

  PongGame? _game;
  BoxConstraints? _gameWidgetConstraints;

  @override
  void initState() {
    super.initState();
    _subscriptionManager.init();
    _pageContext.init().then(
          (pageController) => setState(() => _pageController = pageController),
        );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('TruQuest'),
        actions: [
          StatusPanel(),
        ],
      ),
      body: _pageController == null
          ? Center(child: CircularProgressIndicator())
          : Row(
              children: [
                SideMenu(
                  controller: _pageController!,
                  style: SideMenuStyle(
                    displayMode: SideMenuDisplayMode.open,
                  ),
                  title: SizedBox(
                    width: double.infinity,
                    height: 200,
                    child: Center(
                      child: StreamBuilder<Stream<int>?>(
                        stream: _notificationBloc.progress$$,
                        builder: (context, snapshot) {
                          if (snapshot.data == null) {
                            return Text('Smth');
                          }

                          var progress$ = snapshot.data;
                          return StreamBuilder<int>(
                            stream: progress$,
                            initialData: 10,
                            builder: (context, snapshot) {
                              var percent = snapshot.data!.toDouble();
                              return SleekCircularSlider(
                                initialValue: percent,
                                appearance: CircularSliderAppearance(
                                  size: 150,
                                ),
                              );
                            },
                          );
                        },
                      ),
                    ),
                  ),
                  items: [
                    SideMenuItem(
                      priority: 0,
                      icon: Icon(Icons.person_pin),
                      title: 'Subjects',
                      onTap: () => _pageContext.goto('/subjects'),
                    ),
                    SideMenuItem(
                      priority: 1,
                      icon: Icon(Icons.note),
                      title: 'Things',
                      onTap: () => _pageContext.goto('/things'),
                    ),
                    SideMenuItem(
                      priority: 2,
                      icon: Icon(Icons.question_answer),
                      title: 'How To',
                      onTap: () => _pageContext.goto('/how-to'),
                    ),
                    SideMenuItem(
                      priority: 3,
                      icon: Icon(Icons.theater_comedy_outlined),
                      title: 'Pong!',
                      onTap: () => _pageContext.goto('/pong'),
                    ),
                    SideMenuItem(
                      priority: 4,
                      icon: Icon(Icons.route),
                      title: 'Go To',
                      onTap: () => _pageContext.goto('/goto'),
                    ),
                  ],
                ),
                Expanded(
                  child: PageView.builder(
                    controller: _pageController,
                    physics: NeverScrollableScrollPhysics(),
                    itemBuilder: (context, index) {
                      if (index == 0) {
                        return SubjectsPage();
                      } else if (index == 1) {
                        return Center(
                          child: Text('Things'),
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
                        return Center(
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
                        );
                      }

                      var route = _pageContext.currentRoute;
                      var routeSplit = route.split('/');
                      if (routeSplit[1] == 'subjects') {
                        var subjectId = routeSplit.last;
                        return SubjectPage(subjectId: subjectId);
                      } else if (routeSplit[1] == 'things') {
                        var thingId = routeSplit.last;
                        return ThingPage(thingId: thingId);
                      } else if (routeSplit[1] == 'proposals') {
                        var proposalId = routeSplit.last;
                        return SettlementProposalPage(proposalId: proposalId);
                      }

                      return Center(child: Text('Not Found'));
                    },
                  ),
                ),
              ],
            ),
    );
  }
}
