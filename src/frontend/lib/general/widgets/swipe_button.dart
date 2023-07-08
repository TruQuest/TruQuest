import 'package:flutter/material.dart';

import '../../widget_extensions.dart';

enum _SwipeButtonType {
  swipe,
  expand,
}

class _SwipeButton extends StatefulWidget {
  final Widget child;
  final Widget? thumb;

  final Color? activeThumbColor;
  final Color? inactiveThumbColor;
  final EdgeInsets thumbPadding;

  final Color? activeTrackColor;
  final Color? inactiveTrackColor;
  final EdgeInsets trackPadding;

  final BorderRadius? borderRadius;

  final double width;
  final double height;

  final bool enabled;

  final double elevationThumb;
  final double elevationTrack;

  final VoidCallback? onSwipeStart;
  final VoidCallback? onSwipe;
  final VoidCallback? onSwipeEnd;

  final _SwipeButtonType _swipeButtonType;

  final Duration duration;

  final bool swiped;

  const _SwipeButton.expand({
    Key? key,
    required this.child,
    this.thumb,
    this.activeThumbColor,
    this.inactiveThumbColor,
    this.thumbPadding = EdgeInsets.zero,
    this.activeTrackColor,
    this.inactiveTrackColor,
    this.trackPadding = EdgeInsets.zero,
    this.borderRadius,
    this.width = double.infinity,
    this.height = 50,
    this.enabled = true,
    this.elevationThumb = 0,
    this.elevationTrack = 0,
    this.onSwipeStart,
    this.onSwipe,
    this.onSwipeEnd,
    this.duration = const Duration(milliseconds: 250),
    this.swiped = false,
  })  : assert(elevationThumb >= 0.0),
        assert(elevationTrack >= 0.0),
        _swipeButtonType = _SwipeButtonType.expand,
        super(key: key);

  @override
  State<_SwipeButton> createState() => _SwipeButtonState();
}

class _SwipeButtonState extends State<_SwipeButton>
    with TickerProviderStateMixin {
  late AnimationController swipeAnimationController;
  late AnimationController expandAnimationController;

  late bool swiped;

  @override
  void initState() {
    super.initState();
    _initAnimationControllers();
    swiped = widget.swiped;
  }

  void _initAnimationControllers() {
    swipeAnimationController = AnimationController(
      vsync: this,
      duration: widget.duration,
      lowerBound: 0,
      upperBound: 1,
      value: 0,
    );
    expandAnimationController = AnimationController(
      vsync: this,
      duration: widget.duration,
      lowerBound: 0,
      upperBound: 1,
      value: widget.swiped ? 1 : 0,
    );
  }

  @override
  void didUpdateWidget(covariant _SwipeButton oldWidget) {
    super.didUpdateWidget(oldWidget);

    if (oldWidget.duration != widget.duration) {
      _initAnimationControllers();
    }

    swiped = widget.swiped;
    if (swiped != oldWidget.swiped) {
      if (swiped && expandAnimationController.value != 1) {
        expandAnimationController.animateTo(1);
      } else if (!swiped && expandAnimationController.value != 0) {
        expandAnimationController.animateTo(0);
      }
    }
  }

  @override
  void dispose() {
    swipeAnimationController.dispose();
    expandAnimationController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: widget.width,
      height: widget.height,
      child: LayoutBuilder(
        builder: (context, constraints) {
          return Stack(
            clipBehavior: Clip.none,
            children: [
              _buildTrack(context, constraints),
              _buildThumb(context, constraints),
            ],
          );
        },
      ),
    );
  }

  Widget _buildTrack(BuildContext context, BoxConstraints constraints) {
    final ThemeData theme = Theme.of(context);

    final trackColor = widget.enabled
        ? widget.activeTrackColor ?? theme.colorScheme.background
        : widget.inactiveTrackColor ?? theme.disabledColor;

    final borderRadius = widget.borderRadius ?? BorderRadius.circular(150);
    final elevationTrack = widget.enabled ? widget.elevationTrack : 0.0;

    return Padding(
      padding: widget.trackPadding,
      child: Material(
        elevation: elevationTrack,
        borderRadius: borderRadius,
        clipBehavior: Clip.antiAlias,
        color: trackColor,
        child: Container(
          width: constraints.maxWidth,
          height: widget.height,
          decoration: BoxDecoration(
            borderRadius: borderRadius,
          ),
          clipBehavior: Clip.antiAlias,
          alignment: Alignment.center,
          child: widget.child,
        ),
      ),
    );
  }

  Widget _buildThumb(BuildContext context, BoxConstraints constraints) {
    final ThemeData theme = Theme.of(context);

    final thumbColor = widget.enabled
        ? widget.activeThumbColor ?? theme.colorScheme.secondary
        : widget.inactiveThumbColor ?? theme.disabledColor;

    final borderRadius = widget.borderRadius ?? BorderRadius.circular(150);

    final elevationThumb = widget.enabled ? widget.elevationThumb : 0.0;

    return AnimatedBuilder(
      animation: swipeAnimationController,
      builder: (context, child) {
        return Transform(
          transform: Matrix4.identity()
            ..translate(swipeAnimationController.value *
                (constraints.maxWidth - widget.height)),
          child: Container(
            padding: widget.thumbPadding,
            child: GestureDetector(
              onHorizontalDragStart:
                  widget.enabled ? _onHorizontalDragStart : null,
              onHorizontalDragUpdate: widget.enabled
                  ? (details) =>
                      _onHorizontalDragUpdate(details, constraints.maxWidth)
                  : null,
              onHorizontalDragEnd: widget.enabled ? _onHorizontalDragEnd : null,
              child: Material(
                elevation: elevationThumb,
                borderRadius: borderRadius,
                color: thumbColor,
                clipBehavior: Clip.antiAlias,
                child: AnimatedBuilder(
                  animation: expandAnimationController,
                  builder: (context, child) {
                    return SizedBox(
                      width: widget.height +
                          (expandAnimationController.value *
                              (constraints.maxWidth - widget.height)) -
                          widget.thumbPadding.horizontal,
                      height: widget.height - widget.thumbPadding.vertical,
                      child: widget.thumb ??
                          Icon(
                            Icons.arrow_forward,
                            color: widget.activeTrackColor ??
                                widget.inactiveTrackColor,
                          ),
                    );
                  },
                ),
              ),
            ),
          ),
        );
      },
    );
  }

  _onHorizontalDragStart(DragStartDetails details) {
    setState(() {
      swiped = false;
    });
    widget.onSwipeStart?.call();
  }

  _onHorizontalDragUpdate(DragUpdateDetails details, double width) {
    switch (widget._swipeButtonType) {
      case _SwipeButtonType.swipe:
        if (!swiped && widget.enabled) {
          swipeAnimationController.value +=
              details.primaryDelta! / (width - widget.height);
          if (swipeAnimationController.value == 1) {
            setState(() {
              swiped = true;
              widget.onSwipe?.call();
            });
          }
        }
        break;
      case _SwipeButtonType.expand:
        if (!swiped && widget.enabled) {
          expandAnimationController.value +=
              details.primaryDelta! / (width - widget.height);
          if (expandAnimationController.value == 1) {
            setState(() {
              swiped = true;
              widget.onSwipe?.call();
            });
          }
        }
        break;
    }
  }

  _onHorizontalDragEnd(DragEndDetails details) {
    if (!swiped) {
      setState(() {
        expandAnimationController.animateTo(0);
      });
    }
    widget.onSwipeEnd?.call();
  }
}

class SwipeButton extends StatefulWidget {
  final String text;
  final bool enabled;
  final bool swiped;
  final Future<bool> Function() onCompletedSwipe;

  const SwipeButton({
    super.key,
    required this.text,
    required this.enabled,
    required this.swiped,
    required this.onCompletedSwipe,
  }) : assert(!(enabled && swiped));

  @override
  State<SwipeButton> createState() => SwipeButtonState();
}

class SwipeButtonState extends StateX<SwipeButton> {
  late bool _enabled;
  late bool _swiped;

  @override
  void initState() {
    super.initState();
    _enabled = widget.enabled;
    _swiped = widget.swiped;
  }

  @override
  Widget buildX(BuildContext context) {
    return _SwipeButton.expand(
      enabled: _enabled,
      swiped: _swiped,
      thumb: const Icon(
        Icons.double_arrow_rounded,
        color: Colors.white,
      ),
      child: Text(
        widget.text,
        style: const TextStyle(
          color: Colors.white,
        ),
      ),
      activeThumbColor: Colors.red,
      activeTrackColor: Colors.grey,
      inactiveThumbColor: Colors.blue[300],
      inactiveTrackColor: Colors.grey,
      onSwipe: () async {
        setState(() {
          _enabled = false;
          _swiped = true;
        });

        if (!await widget.onCompletedSwipe()) {
          setState(() {
            _enabled = true;
            _swiped = false;
          });
        }
      },
    );
  }
}
