import 'package:flutter/material.dart';

class SwipeButton extends StatefulWidget {
  final Widget onTrackChild;
  final Widget? onExpandingHandleChild;
  final double width;
  final double height;
  final bool enabled;
  final bool swiped;
  final Color color;
  final Color disabledColor;
  final Color trackColor;
  final Duration rollbackDuration;
  final Curve rollbackCurve;
  final Future<bool> Function() onFullSwipe;

  const SwipeButton({
    super.key,
    required this.onTrackChild,
    this.onExpandingHandleChild,
    required this.width,
    required this.height,
    required this.enabled,
    required this.swiped,
    required this.color,
    required this.disabledColor,
    required this.trackColor,
    this.rollbackDuration = const Duration(milliseconds: 500),
    this.rollbackCurve = Curves.fastOutSlowIn,
    required this.onFullSwipe,
  });

  static Widget expand({
    Key? key,
    required Widget onTrackChild,
    Widget? onExpandingHandleChild,
    required double height,
    required bool enabled,
    required bool swiped,
    required Color color,
    required Color disabledColor,
    required Color trackColor,
    Duration rollbackDuration = const Duration(milliseconds: 500),
    Curve rollbackCurve = Curves.fastOutSlowIn,
    required Future<bool> Function() onFullSwipe,
  }) =>
      LayoutBuilder(
        builder: (_, constraints) => SwipeButton(
          key: key,
          onTrackChild: onTrackChild,
          onExpandingHandleChild: onExpandingHandleChild,
          width: constraints.maxWidth,
          height: height,
          enabled: enabled,
          swiped: swiped,
          color: color,
          disabledColor: disabledColor,
          trackColor: trackColor,
          rollbackDuration: rollbackDuration,
          rollbackCurve: rollbackCurve,
          onFullSwipe: onFullSwipe,
        ),
      );

  @override
  State<SwipeButton> createState() => _SwipeButtonState();
}

class _SwipeButtonState extends State<SwipeButton> with SingleTickerProviderStateMixin {
  late final AnimationController _animationController;

  late bool _enabled;
  late bool _swiped;
  late Color _color;

  late double _upperBound;

  bool _dragging = false;
  late double _dragStartDx;
  bool _inCooldown = false;

  @override
  void initState() {
    super.initState();

    _enabled = widget.enabled;
    _swiped = widget.swiped;
    _color = _enabled ? widget.color : widget.disabledColor;
    _upperBound = widget.width - widget.height;

    _animationController = AnimationController(
      lowerBound: 0,
      upperBound: _upperBound,
      value: _swiped ? _upperBound : 0,
      vsync: this,
    );
  }

  @override
  void dispose() {
    _animationController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        Container(
          decoration: BoxDecoration(
            color: widget.trackColor,
            borderRadius: BorderRadius.circular(widget.height * 0.5),
          ),
          width: widget.width,
          height: widget.height,
          alignment: Alignment.center,
          child: widget.onTrackChild,
        ),
        Stack(
          children: [
            AnimatedBuilder(
              animation: _animationController,
              builder: (context, _) => Container(
                decoration: BoxDecoration(
                  color: _color,
                  borderRadius: BorderRadius.circular(widget.height * 0.5),
                ),
                width: widget.height + _animationController.value,
                height: widget.height,
                alignment: Alignment.center,
                child: widget.onExpandingHandleChild,
              ),
            ),
            Positioned(
              right: 0,
              child: MouseRegion(
                cursor: SystemMouseCursors.click,
                child: GestureDetector(
                  onHorizontalDragStart: (details) {
                    if (_dragging || _inCooldown || _swiped || !_enabled) return;
                    _dragging = true;
                    _dragStartDx = details.globalPosition.dx;
                  },
                  onHorizontalDragUpdate: (details) {
                    if (!_dragging) return;
                    var delta = details.globalPosition.dx - _dragStartDx;
                    if (delta > 0) {
                      _animationController.value = delta < _upperBound ? delta : _upperBound;
                    }
                  },
                  onHorizontalDragEnd: (details) async {
                    if (!_dragging) return;

                    _dragging = false;

                    if (_animationController.value < _animationController.upperBound) {
                      _inCooldown = true;
                      await _animationController.animateTo(
                        _animationController.lowerBound,
                        duration: widget.rollbackDuration,
                        curve: widget.rollbackCurve,
                      );
                      _inCooldown = false;

                      return;
                    }

                    _swiped = true;

                    if (await widget.onFullSwipe()) {
                      if (mounted) {
                        setState(() {
                          _enabled = false;
                          _color = widget.disabledColor;
                        });
                      }
                    } else {
                      _swiped = false;
                      _inCooldown = true;
                      await _animationController.animateTo(
                        _animationController.lowerBound,
                        duration: widget.rollbackDuration,
                        curve: widget.rollbackCurve,
                      );
                      _inCooldown = false;
                    }
                  },
                  child: Container(
                    width: widget.height,
                    height: widget.height,
                    decoration: const BoxDecoration(
                      color: Colors.transparent,
                      shape: BoxShape.circle,
                    ),
                  ),
                ),
              ),
            ),
          ],
        ),
      ],
    );
  }
}
