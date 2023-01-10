import 'package:flutter/material.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

class ProgressReport extends StatefulWidget {
  final Stream<int> progress$;

  const ProgressReport({super.key, required this.progress$});

  @override
  State<ProgressReport> createState() => _ProgressReportState();
}

class _ProgressReportState extends State<ProgressReport> {
  @override
  void initState() {
    super.initState();
    widget.progress$.listen(null, onDone: () {
      Navigator.of(context).pop();
    });
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      backgroundColor: Colors.transparent,
      title: Text(
        'Archiving in progress...',
        style: TextStyle(color: Colors.white),
      ),
      content: StreamBuilder<int>(
        stream: widget.progress$,
        initialData: 10,
        builder: (context, snapshot) {
          var percent = snapshot.data!.toDouble();
          return SleekCircularSlider(
            initialValue: percent,
          );
        },
      ),
    );
  }
}
