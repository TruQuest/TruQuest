import 'package:easy_sidemenu/easy_sidemenu.dart';
import 'package:flutter/material.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

import '../widgets/image_block_with_crop.dart';
import '../bloc/notification_bloc.dart';
import '../../widget_extensions.dart';
import '../widgets/document_composer.dart';
import '../widgets/evidence_block.dart';
import '../widgets/image_block.dart';
import '../widgets/status_panel.dart';
import '../widgets/tags_block.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends StateX<HomePage> {
  late final _notificationBloc = use<NotificationBloc>();

  final _pageController = PageController();

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
            ],
          ),
          Expanded(
            child: PageView.builder(
              controller: _pageController,
              itemCount: 2,
              itemBuilder: (context, index) => Center(
                child: Text('Page $index'),
              ),
            ),
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton(
        child: Icon(Icons.edit),
        onPressed: () {
          showDialog(
            context: context,
            barrierDismissible: false,
            builder: (_) => UseScope(
              child: DocumentComposer(
                sideBlocks: [
                  ImageBlockWithCrop(),
                  TagsBlock(),
                  EvidenceBlock(),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}
