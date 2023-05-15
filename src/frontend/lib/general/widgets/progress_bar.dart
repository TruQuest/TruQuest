import 'package:flutter/material.dart';
import 'package:flutter_animation_progress_bar/flutter_animation_progress_bar.dart';
import 'package:loading_animation_widget/loading_animation_widget.dart';

import '../bloc/notification_bloc.dart';
import '../../widget_extensions.dart';

class ProgressBar extends StatefulWidget {
  const ProgressBar({super.key});

  @override
  State<ProgressBar> createState() => _ProgressBarState();
}

class _ProgressBarState extends StateX<ProgressBar> {
  late final _notificationBloc = use<NotificationBloc>();

  @override
  Widget build(BuildContext context) {
    return StreamBuilder(
      stream: _notificationBloc.progress$$,
      builder: (context, snapshot) {
        if (snapshot.data == null) return SizedBox.shrink();

        var progress$ = snapshot.data!;

        return Row(
          children: [
            LoadingAnimationWidget.discreteCircle(
              color: Colors.white,
              size: 20,
            ),
            SizedBox(width: 8),
            Expanded(
              child: StreamBuilder(
                stream: progress$,
                builder: (context, snapshot) => FAProgressBar(
                  currentValue: (snapshot.data ?? 0).toDouble(),
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
