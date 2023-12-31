import 'dart:async';
import 'dart:math';

import 'package:flutter/material.dart';
import 'package:auto_size_text/auto_size_text.dart';
import 'package:google_fonts/google_fonts.dart';

import '../contexts/page_context.dart';
import '../bloc/notification_actions.dart';
import '../bloc/notification_bloc.dart';
import '../../widget_extensions.dart';
import '../models/vm/notification_vm.dart';
import 'clipped_rect.dart';

class NotificationTracker extends StatefulWidget {
  const NotificationTracker({super.key});

  @override
  State<NotificationTracker> createState() => _NotificationTrackerState();
}

class _NotificationTrackerState extends StateX<NotificationTracker> with TickerProviderStateMixin {
  late final _notificationBloc = use<NotificationBloc>();
  late final _pageContext = use<PageContext>();

  late final AnimationController _overlayAnimationController;
  late Animation<double> _overlayAnimation;
  OverlayEntry? _overlayEntry;
  final double _minOverlayWidth = 240;
  late double _overlayWidth;

  late final AnimationController _iconAnimationController;
  late final StreamSubscription<(List<NotificationVm>, String?)> _unreadNotifications$$;

  Future _latestAnimateIconFuture = Future.value();
  int _unreadCount = 0;

  @override
  void initState() {
    super.initState();

    _overlayAnimationController = AnimationController(
      duration: const Duration(milliseconds: 250),
      vsync: this,
    );

    _iconAnimationController = AnimationController(
      duration: const Duration(milliseconds: 500),
      vsync: this,
    );

    _unreadNotifications$$ = _notificationBloc.unreadNotifications$.listen((data) {
      var (notifications, _) = data;
      if (notifications.length > _unreadCount) {
        _latestAnimateIconFuture = _latestAnimateIconFuture.then((_) => _animateIcon());
      }

      if (notifications.length != _unreadCount) {
        setState(() => _unreadCount = notifications.length);
      }
    });
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();

    // @@NOTE: When screen size changes this state gets disposed of, so we don't try to keep the panel open
    // if it was open before the change. Also, before the disposal of the old state and subsequent creation of
    // a new one, 'didChangeDependencies' fires on the old state because of 'MediaQuery' dependency. That's why
    // _overlayWidth and _overlayAnimation are not 'final'.

    var screenWidth = MediaQuery.sizeOf(context).width;
    _overlayWidth = max(screenWidth * 0.2, _minOverlayWidth);
    _overlayAnimation = Tween<double>(begin: 0, end: _overlayWidth).animate(
      CurvedAnimation(
        parent: _overlayAnimationController,
        curve: Curves.fastOutSlowIn,
      ),
    );
  }

  void _removeNotificationPanel() {
    _overlayEntry?.remove();
    _overlayEntry?.dispose();
    _overlayEntry = null;
  }

  @override
  void dispose() {
    _iconAnimationController.dispose();
    _unreadNotifications$$.cancel();
    _removeNotificationPanel();
    _overlayAnimationController.dispose();
    super.dispose();
  }

  Future _animateIcon() async {
    for (int i = 0; i < 3; ++i) {
      await _iconAnimationController.forward();
      await _iconAnimationController.reverse();
    }
  }

  void _buildNotificationPanel() {
    _overlayEntry = OverlayEntry(
      builder: (_) => Stack(
        children: [
          Positioned.fill(
            child: GestureDetector(
              onTap: () async {
                await _overlayAnimationController.reverse();
                _removeNotificationPanel();
              },
              child: Container(color: Colors.black.withOpacity(0.2)),
            ),
          ),
          AnimatedBuilder(
            animation: _overlayAnimation,
            child: Material(
              color: Colors.black54,
              elevation: 5,
              child: StreamBuilder(
                stream: _notificationBloc.unreadNotifications$,
                builder: (context, snapshot) {
                  if (snapshot.data == null || snapshot.data!.$1.isEmpty) {
                    return const Center(
                      child: Text(
                        'Nothing here yet',
                        style: TextStyle(color: Colors.white),
                      ),
                    );
                  }

                  var (notifications, userId) = snapshot.data!;

                  return Column(
                    children: [
                      Card(
                        margin: const EdgeInsets.fromLTRB(8, 12, 8, 12),
                        elevation: 5,
                        child: Padding(
                          padding: const EdgeInsets.all(6),
                          child: Row(
                            children: [
                              const SizedBox(width: 40),
                              Expanded(
                                child: Text(
                                  '${notifications.length} notification(s)',
                                  style: GoogleFonts.philosopher(
                                    fontSize: 18,
                                  ),
                                  textAlign: TextAlign.center,
                                ),
                              ),
                              IconButton(
                                icon: const Icon(
                                  Icons.delete_sweep,
                                  color: Colors.red,
                                ),
                                onPressed: () => _notificationBloc.dispatch(
                                  Dismiss(
                                    notifications: notifications,
                                    userId: userId,
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
                      Expanded(
                        child: ListView.builder(
                          itemCount: notifications.length,
                          itemBuilder: (context, index) {
                            var notification = notifications[index];
                            return InkWell(
                              onTap: () {
                                _notificationBloc.dispatch(Dismiss(
                                  notifications: [notification],
                                  userId: userId,
                                ));

                                _pageContext.goto(notification.itemRoute);
                                Navigator.of(context).pop();
                              },
                              child: Card(
                                margin: const EdgeInsets.fromLTRB(16, 12, 16, 12),
                                elevation: 5,
                                shadowColor: Colors.white,
                                child: Padding(
                                  padding: const EdgeInsets.fromLTRB(10, 10, 2, 10),
                                  child: Row(
                                    children: [
                                      Expanded(
                                        child: Column(
                                          crossAxisAlignment: CrossAxisAlignment.start,
                                          children: [
                                            AutoSizeText(
                                              notification.title,
                                              style: GoogleFonts.philosopher(
                                                color: Colors.black,
                                                fontSize: 16,
                                              ),
                                              maxLines: 2,
                                              overflow: TextOverflow.ellipsis,
                                            ),
                                            const SizedBox(height: 6),
                                            Text(
                                              notification.details ?? '',
                                              style: GoogleFonts.raleway(
                                                color: Colors.black87,
                                              ),
                                              maxLines: 2,
                                            ),
                                          ],
                                        ),
                                      ),
                                      IconButton(
                                        icon: const Icon(
                                          Icons.delete,
                                          color: Colors.red,
                                        ),
                                        onPressed: () => _notificationBloc.dispatch(
                                          Dismiss(
                                            notifications: [notification],
                                            userId: userId,
                                          ),
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                              ),
                            );
                          },
                        ),
                      ),
                    ],
                  );
                },
              ),
            ),
            builder: (_, child) => Positioned(
              top: 0,
              bottom: 0,
              left: _overlayAnimation.value - _overlayWidth,
              width: _overlayWidth,
              child: child!,
            ),
          ),
        ],
      ),
    );

    Overlay.of(context).insert(_overlayEntry!);
  }

  @override
  Widget buildX(BuildContext context) {
    return Stack(
      children: [
        const ClippedRect(
          height: 50,
          color: Color(0xffF8F9FA),
          fromNarrowToWide: false,
          narrowSideFraction: 0.4,
        ),
        Positioned(
          top: 2,
          left: 6,
          child: RotationTransition(
            turns: Tween(begin: 0.0, end: -0.13)
                .chain(CurveTween(curve: Curves.elasticIn))
                .animate(_iconAnimationController),
            child: IconButton(
              icon: Icon(
                _unreadCount == 0 ? Icons.notifications : Icons.notifications_active,
                color: const Color.fromARGB(255, 208, 53, 76),
              ),
              iconSize: 26,
              onPressed: () async {
                _buildNotificationPanel();
                await _overlayAnimationController.forward();
              },
            ),
          ),
        ),
        if (_unreadCount > 0)
          Positioned(
            top: 8,
            left: 40,
            child: Text(
              _unreadCount.toString(),
              style: TextStyle(
                color: Colors.red[800],
                fontSize: 12,
              ),
            ),
          )
      ],
    );
  }
}
