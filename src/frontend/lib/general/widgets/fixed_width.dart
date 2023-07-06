import 'package:flutter/material.dart';

class FixedWidth extends StatefulWidget {
  final double width;
  final Widget child;

  const FixedWidth({
    super.key,
    required this.width,
    required this.child,
  });

  @override
  State<FixedWidth> createState() => _FixedWidthState();
}

class _FixedWidthState extends State<FixedWidth> {
  final _horizontalController = ScrollController();

  @override
  void dispose() {
    _horizontalController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(builder: (context, constraints) {
      if (constraints.maxWidth < widget.width) {
        return Scrollbar(
          controller: _horizontalController,
          thumbVisibility: true,
          child: SingleChildScrollView(
            controller: _horizontalController,
            scrollDirection: Axis.horizontal,
            physics: const AlwaysScrollableScrollPhysics(),
            child: UnconstrainedBox(
              child: SizedBox(
                width: widget.width,
                height: constraints.maxHeight,
                child: widget.child,
              ),
            ),
          ),
        );
      }

      return Align(
        alignment: Alignment.topCenter,
        child: SizedBox(
          width: widget.width,
          height: constraints.maxHeight,
          child: widget.child,
        ),
      );
    });
  }
}
