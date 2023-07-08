import 'dart:async';

import 'package:flutter/material.dart';
import 'package:auto_size_text/auto_size_text.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:lottie/lottie.dart';
// ignore: implementation_imports
import 'package:lottie/src/model/layer/layer.dart';
import 'package:side_sheet/side_sheet.dart';

import '../../ethereum/bloc/ethereum_bloc.dart';
import '../../ethereum/bloc/ethereum_actions.dart';
import '../contexts/page_context.dart';
import '../bloc/notification_actions.dart';
import '../bloc/notification_bloc.dart';
import '../services/notifications_cache.dart';
import '../../widget_extensions.dart';
import 'clipped_rect.dart';

class NotificationTracker extends StatefulWidget {
  const NotificationTracker({super.key});

  @override
  State<NotificationTracker> createState() => _NotificationTrackerState();
}

class _NotificationTrackerState extends StateX<NotificationTracker>
    with SingleTickerProviderStateMixin {
  late final _notificationBloc = use<NotificationBloc>();
  // @@??: Should go through the bloc instead of exposing directly ?
  late final _notificationsCache = use<NotificationsCache>();
  late final _ethereumBloc = use<EthereumBloc>();
  late final _pageContext = use<PageContext>();

  late final AnimationController _animationController;
  late final StreamSubscription<int> _unreadNotificationsCount$$;

  LottieComposition? _composition;

  @override
  void initState() {
    super.initState();

    _animationController = AnimationController(
      vsync: this,
      duration: const Duration(seconds: 1),
    );

    AssetLottie('assets/icons/bell.json').load().then((composition) {
      var oldLayer = composition.layers[1];
      composition.layers[1] = Layer(
        shapes: oldLayer.shapes,
        composition: oldLayer.composition,
        name: oldLayer.name,
        id: oldLayer.id,
        layerType: oldLayer.layerType,
        parentId: oldLayer.parentId,
        refId: oldLayer.refId,
        masks: oldLayer.masks,
        transform: oldLayer.transform,
        solidWidth: oldLayer.solidWidth,
        solidHeight: oldLayer.solidHeight,
        solidColor: const Color.fromARGB(255, 208, 53, 76),
        timeStretch: oldLayer.timeStretch,
        startFrame: oldLayer.startFrame,
        preCompWidth: oldLayer.preCompWidth,
        preCompHeight: oldLayer.preCompHeight,
        text: oldLayer.text,
        textProperties: oldLayer.textProperties,
        inOutKeyframes: oldLayer.inOutKeyframes,
        matteType: oldLayer.matteType,
        timeRemapping: oldLayer.timeRemapping,
        isHidden: oldLayer.isHidden,
        blurEffect: oldLayer.blurEffect,
        dropShadowEffect: oldLayer.dropShadowEffect,
      );

      setState(() {
        _composition = composition;
      });

      _unreadNotificationsCount$$ =
          _notificationsCache.unreadNotificationsCount$.listen((_) {
        _animationController.reset();
        _animationController.forward();
      });
    });
  }

  @override
  void dispose() {
    _animationController.dispose();
    _unreadNotificationsCount$$.cancel();
    super.dispose();
  }

  Widget _buildNotificationPanel() {
    return StreamBuilder(
      stream: _notificationsCache.unreadNotifications$,
      builder: (context, snapshot) {
        if (snapshot.data == null || snapshot.data!.$1.isEmpty) {
          return const Center(
            child: Text(
              'Nothing here yet',
              style: TextStyle(color: Colors.white),
            ),
          );
        }

        var (notifications, username) = snapshot.data!;

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
                          username: username,
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
                      if (notification.itemId == '') {
                        // client-side warning
                        if (notification.itemUpdateCategory == 1) {
                          // unsupported chain
                          _ethereumBloc.dispatch(const SwitchEthereumChain());
                        }
                        return;
                      }

                      _notificationBloc.dispatch(Dismiss(
                        notifications: [notification],
                        username: username,
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
                            if (notification.itemId != '')
                              IconButton(
                                icon: const Icon(
                                  Icons.delete,
                                  color: Colors.red,
                                ),
                                onPressed: () => _notificationBloc.dispatch(
                                  Dismiss(
                                    notifications: [notification],
                                    username: username,
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
    );
  }

  @override
  Widget buildX(BuildContext context) {
    return Stack(
      children: [
        const ClippedRect(
          height: 50,
          color: Color(0xffF8F9FA),
          fromNarrowToWide: false,
        ),
        if (_composition != null)
          IconButton(
            icon: Lottie(
              composition: _composition,
              controller: _animationController,
            ),
            iconSize: 35,
            onPressed: () => SideSheet.left(
              context: context,
              sheetColor: Colors.black54,
              width: MediaQuery.of(context).size.width * 0.2,
              body: _buildNotificationPanel(),
            ),
          ),
        StreamBuilder(
          stream: _notificationsCache.unreadNotificationsCount$,
          initialData: 0,
          builder: (context, snapshot) {
            var count = snapshot.data!;
            return Positioned(
              top: 8,
              left: 36,
              child: count == -1
                  ? Text(
                      '!',
                      style: GoogleFonts.exo2(
                        color: Colors.red[800],
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                      ),
                    )
                  : count > 0
                      ? Text(
                          count.toString(),
                          style: TextStyle(
                            color: Colors.red[800],
                            fontSize: 12,
                          ),
                        )
                      : const SizedBox.shrink(),
            );
          },
        ),
      ],
    );
  }
}
