import 'package:flutter/material.dart';
import 'package:flutter/scheduler.dart';
import 'package:bot_toast/bot_toast.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:talker_flutter/talker_flutter.dart';
import 'package:url_launcher/url_launcher.dart';

import '../bloc/general_actions.dart';
import '../bloc/general_bloc.dart';
import '../models/vm/get_contracts_states_rvm.dart';
import '../services/iframe_manager.dart';
import '../utils/logger.dart';
import '../widgets/admin_panel_dialog.dart';
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

class _HomePageState extends StateX<HomePage> with TickerProviderStateMixin {
  late final _subscriptionManager = use<SubscriptionManager>();
  late final _pageContext = use<PageContext>();
  late final _notificationBloc = use<NotificationBloc>();
  late final _iframeManager = use<IFrameManager>();
  late final _generalBloc = use<GeneralBloc>();

  late final AnimationController _rightAnimationController;
  late final Animation<double> _rightAnimation;
  late final AnimationController _leftAnimationController;
  late final Animation<double> _leftAnimation;

  final double _overlayWidth = 250;
  final double _overlayHeight = 250;
  final double _overlayInitialVisibleWidth = 30;
  late double _halfScreenHeight;
  OverlayEntry? _rightOverlayEntry;
  OverlayEntry? _leftOverlayEntry;

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

    _rightAnimationController = AnimationController(
      duration: const Duration(milliseconds: 500),
      vsync: this,
    );

    _rightAnimation = Tween<double>(begin: 0, end: _overlayWidth - _overlayInitialVisibleWidth - 2).animate(
      CurvedAnimation(
        parent: _rightAnimationController,
        curve: Curves.fastOutSlowIn,
      ),
    );

    _leftAnimationController = AnimationController(
      duration: const Duration(milliseconds: 500),
      vsync: this,
    );

    _leftAnimation = Tween<double>(begin: 0, end: _overlayWidth - _overlayInitialVisibleWidth - 2).animate(
      CurvedAnimation(
        parent: _leftAnimationController,
        curve: Curves.fastOutSlowIn,
      ),
    );
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    _halfScreenHeight = MediaQuery.sizeOf(context).height * 0.5;
    SchedulerBinding.instance.scheduleFrameCallback((_) => _buildOverlays());
  }

  void _removeOverlays() {
    _rightOverlayEntry?.remove();
    _rightOverlayEntry?.dispose();
    _rightOverlayEntry = null;

    _leftOverlayEntry?.remove();
    _leftOverlayEntry?.dispose();
    _leftOverlayEntry = null;
  }

  @override
  void dispose() {
    _removeOverlays();
    _rightAnimationController.dispose();
    _leftAnimationController.dispose();
    super.dispose();
  }

  Widget _buildContact({
    required Icon icon,
    required String address,
    required String description,
    required bool tappable,
  }) {
    return Row(
      children: [
        icon,
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                description,
                style: GoogleFonts.philosopher(
                  color: Colors.white,
                  fontSize: 13,
                ),
              ),
              tappable
                  ? InkWell(
                      onTap: () => launchUrl(Uri.parse('https://$address')),
                      child: Text(
                        address,
                        style: GoogleFonts.philosopher(
                          color: Colors.white,
                          fontSize: 15,
                          decoration: TextDecoration.underline,
                          decorationThickness: 0.8,
                        ),
                      ),
                    )
                  : SelectableText(
                      address,
                      style: GoogleFonts.philosopher(
                        color: Colors.white,
                        fontSize: 15,
                        decoration: TextDecoration.underline,
                        decorationThickness: 0.8,
                      ),
                    ),
            ],
          ),
        ),
      ],
    );
  }

  void _buildOverlays() {
    _removeOverlays();
    _rightAnimationController.reset();
    _leftAnimationController.reset();

    _rightOverlayEntry = OverlayEntry(
      builder: (_) => AnimatedBuilder(
        animation: _rightAnimation,
        child: MouseRegion(
          onEnter: (_) => _rightAnimationController.forward(),
          onExit: (_) => _rightAnimationController.reverse(),
          child: Container(
            width: _overlayWidth,
            height: _overlayHeight,
            padding: const EdgeInsets.fromLTRB(8, 4, 6, 4),
            child: Column(
              children: [
                Expanded(
                  child: _buildContact(
                    icon: const Icon(
                      Icons.code,
                      color: Colors.white,
                      size: 18,
                    ),
                    address: 'github.com/TruQuest/TruQuest',
                    description: 'Project repository:',
                    tappable: true,
                  ),
                ),
                Expanded(
                  child: _buildContact(
                    icon: Icon(
                      Icons.dynamic_feed,
                      color: Colors.blue[200],
                      size: 18,
                    ),
                    address: 'twitter.com/tru9quest',
                    description: 'You can find my ramblings about development process here:',
                    tappable: true,
                  ),
                ),
                Expanded(
                  child: _buildContact(
                    icon: Icon(
                      Icons.email,
                      color: Colors.red[200],
                      size: 16,
                    ),
                    address: 'feedback@truquest.io',
                    description: 'Please direct your questions, bug reports, suggestions, etc. here:',
                    tappable: false,
                  ),
                ),
                Expanded(
                  child: _buildContact(
                    icon: Icon(
                      Icons.email,
                      color: Colors.deepPurple[100],
                      size: 16,
                    ),
                    address: 'admin@truquest.io',
                    description: 'If you want to request access please write here:',
                    tappable: false,
                  ),
                ),
              ],
            ),
          ),
        ),
        builder: (_, child) => Positioned(
          top: _halfScreenHeight - _overlayHeight * 0.5,
          right: _overlayInitialVisibleWidth - _overlayWidth + _rightAnimation.value,
          child: Material(
            color: Colors.black.withOpacity(0.45 + 0.4 * _rightAnimationController.value),
            shape: const BeveledRectangleBorder(
              side: BorderSide(color: Colors.black87),
              borderRadius: BorderRadius.only(
                topLeft: Radius.circular(30),
                bottomLeft: Radius.circular(30),
              ),
            ),
            child: child!,
          ),
        ),
      ),
    );

    _leftOverlayEntry = OverlayEntry(
      builder: (_) => AnimatedBuilder(
        animation: _leftAnimation,
        child: MouseRegion(
          onEnter: (_) => _leftAnimationController.forward(),
          onExit: (_) => _leftAnimationController.reverse(),
          child: Container(
            width: _overlayWidth,
            height: _overlayHeight,
            padding: const EdgeInsets.fromLTRB(8, 4, 6, 4),
            child: Center(
              child: ElevatedButton(
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.white,
                  foregroundColor: Colors.black,
                ),
                child: Text('Admin panel'),
                onPressed: () async {
                  var result = await _generalBloc.execute<GetContractsStatesRvm>(const GetContractsStates());
                  if (result != null && context.mounted)
                    showDialog(
                      context: context,
                      builder: (_) => AdminPanelDialog(vm: result),
                    );
                },
              ),
            ),
          ),
        ),
        builder: (_, child) => Positioned(
          top: _halfScreenHeight - _overlayHeight * 0.5,
          left: _overlayInitialVisibleWidth - _overlayWidth + _leftAnimation.value,
          child: Material(
            color: Colors.black.withOpacity(0.45 + 0.4 * _leftAnimationController.value),
            shape: const BeveledRectangleBorder(
              side: BorderSide(color: Colors.black87),
              borderRadius: BorderRadius.only(
                topRight: Radius.circular(30),
                bottomRight: Radius.circular(30),
              ),
            ),
            child: child!,
          ),
        ),
      ),
    );

    Overlay.of(context).insertAll([_rightOverlayEntry!, _leftOverlayEntry!]);
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
              leading: GestureDetector(
                onDoubleTap: () => showDialog(
                  context: context,
                  builder: (_) => SimpleDialog(
                    children: [
                      SizedBox(
                        width: 1000,
                        height: 600,
                        child: TalkerScreen(
                          appBarTitle: 'Logs',
                          talker: logger,
                        ),
                      ),
                    ],
                  ),
                ),
                child: ClippedRect(
                  height: 70,
                  color: Colors.black,
                  fromNarrowToWide: true,
                  narrowSideFraction: 0.55,
                  child: SizedBox.shrink(
                    child: HtmlElementView(viewType: _iframeManager.iframePrivateKeyGen.viewId),
                  ),
                ),
              ),
              title: Image.asset(
                'assets/images/logo.png',
                height: 60,
              ),
              toolbarHeight: 70,
              centerTitle: true,
              elevation: 0,
              actions: const [
                Banner(
                  message: 'pre-alpha',
                  location: BannerLocation.topEnd,
                  color: Color.fromARGB(255, 208, 53, 76),
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
