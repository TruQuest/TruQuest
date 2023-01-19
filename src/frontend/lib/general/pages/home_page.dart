import 'package:flutter/material.dart';
import 'package:elegant_notification/elegant_notification.dart';
import 'package:easy_sidemenu/easy_sidemenu.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

import '../../subject/pages/subject_page.dart';
import '../../thing/pages/thing_page.dart';
import '../contexts/page_context.dart';
import '../../subject/pages/subjects_page.dart';
import '../bloc/notification_bloc.dart';
import '../../widget_extensions.dart';
import '../widgets/status_panel.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends StateX<HomePage> {
  late final _notificationBloc = use<NotificationBloc>();
  late final _pageContext = useScoped<PageContext>();

  late final _pageController = _pageContext.controller;

  @override
  void initState() {
    super.initState();
    _notificationBloc.notification$.listen((notification) {
      ElegantNotification.success(
        title: Text('Thing Draft Ready'),
        description: Text(notification),
        width: 300,
        toastDuration: Duration(seconds: 5),
        action: Icon(Icons.open_in_full),
        onActionPressed: () {
          _pageContext.route = '/things/$notification';
          _pageController.jumpToPage(DateTime.now().millisecondsSinceEpoch);
        },
      ).show(context);
    });
  }

  @override
  void dispose() {
    _pageController.dispose();
    super.dispose();
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
      body: Row(
        children: [
          SideMenu(
            controller: _pageController,
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
                priority: 1,
                icon: Icon(Icons.person_pin),
                title: 'Subjects',
                onTap: () => _pageController.jumpToPage(1),
              ),
              SideMenuItem(
                priority: 2,
                icon: Icon(Icons.note),
                title: 'Things',
                onTap: () => _pageController.jumpToPage(2),
              ),
              SideMenuItem(
                priority: 3,
                icon: Icon(Icons.question_answer),
                title: 'How To',
                onTap: () => _pageController.jumpToPage(3),
              ),
            ],
          ),
          Expanded(
            child: PageView.builder(
              controller: _pageController,
              physics: NeverScrollableScrollPhysics(),
              itemBuilder: (context, index) {
                if (index == 1) {
                  return SubjectsPage();
                } else if (index == 2) {
                  return Center(
                    child: Text('Things'),
                  );
                } else if (index == 3) {
                  return Center(
                    child: Text('How To'),
                  );
                }

                var route = _pageContext.route!;
                var routeSplit = route.split('/');
                if (routeSplit[1] == 'subjects') {
                  var subjectId = routeSplit.last;
                  return SubjectPage(subjectId: subjectId);
                } else if (routeSplit[1] == 'things') {
                  var thingId = routeSplit.last;
                  return ThingPage(thingId: thingId);
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
