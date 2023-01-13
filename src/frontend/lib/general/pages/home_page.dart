import 'package:flutter/material.dart';
import 'package:easy_sidemenu/easy_sidemenu.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

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

  final _pageController = PageController(initialPage: 0);

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
        // actions: [
        //   StatusPanel(),
        // ],
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
                priority: 0,
                icon: Icon(Icons.person_pin),
                title: 'Subjects',
                onTap: () => _pageController.jumpToPage(0),
              ),
              SideMenuItem(
                priority: 1,
                icon: Icon(Icons.note),
                title: 'Things',
                onTap: () => _pageController.jumpToPage(1),
              ),
              SideMenuItem(
                priority: 2,
                icon: Icon(Icons.question_answer),
                title: 'How To',
                onTap: () => _pageController.jumpToPage(2),
              ),
            ],
          ),
          Expanded(
            child: PageView.builder(
              controller: _pageController,
              itemCount: 3,
              itemBuilder: (context, index) {
                if (index == 0) {
                  return SubjectsPage();
                }

                return Center(
                  child: Text('Page $index'),
                );
              },
            ),
          ),
        ],
      ),
    );
  }
}
