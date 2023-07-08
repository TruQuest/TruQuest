import 'package:flutter/material.dart';
import 'package:flutter_animation_progress_bar/flutter_animation_progress_bar.dart';
import 'package:loading_animation_widget/loading_animation_widget.dart';

import '../bloc/notification_bloc.dart';
import '../../widget_extensions.dart';

// ignore: must_be_immutable
class ProgressBar extends StatelessWidgetX {
  late final _notificationBloc = use<NotificationBloc>();

  ProgressBar({super.key});

  @override
  Widget buildX(BuildContext context) {
    return StreamBuilder(
      stream: _notificationBloc.progress$$,
      builder: (context, snapshot) {
        if (snapshot.data == null) return const SizedBox.shrink();

        var progress$ = snapshot.data!;

        return Row(
          children: [
            LoadingAnimationWidget.discreteCircle(
              color: Colors.white,
              size: 20,
            ),
            const SizedBox(width: 8),
            Expanded(
              child: StreamBuilder(
                stream: progress$,
                builder: (context, snapshot) => FAProgressBar(
                  currentValue: (snapshot.data ?? 10).toDouble(),
                  maxValue: 100,
                  displayText: '%',
                ),
              ),
            ),
          ],
        );
      },
    );
  }
}
