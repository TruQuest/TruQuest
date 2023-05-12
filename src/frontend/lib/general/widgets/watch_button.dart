import 'package:flutter/material.dart';

class WatchButton extends StatefulWidget {
  final bool markedAsWatched;
  final void Function(bool markedAsWatched) onPressed;

  const WatchButton({
    super.key,
    required this.markedAsWatched,
    required this.onPressed,
  });

  @override
  State<WatchButton> createState() => _WatchButtonState();
}

class _WatchButtonState extends State<WatchButton> {
  late bool _markedAsWatched;

  @override
  void initState() {
    super.initState();
    _markedAsWatched = widget.markedAsWatched;
  }

  @override
  Widget build(BuildContext context) {
    return IconButton(
      icon: Icon(
        Icons.remove_red_eye,
        color: _markedAsWatched ? Colors.deepOrange[300] : Colors.grey,
      ),
      onPressed: () {
        setState(() {
          _markedAsWatched = !_markedAsWatched;
        });

        widget.onPressed(_markedAsWatched);
      },
    );
  }
}
