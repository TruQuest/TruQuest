import 'dart:async';

import 'package:flutter/material.dart';
import 'package:dropdown_button2/dropdown_button2.dart';
import 'package:google_fonts/google_fonts.dart';

class VoteDialog<T> extends StatefulWidget {
  final List<T> decisions;
  final String Function(T) getDisplayString;
  final FutureOr<void> Function(T decision, String reason) onVote;

  const VoteDialog({
    super.key,
    required this.decisions,
    required this.getDisplayString,
    required this.onVote,
  });

  @override
  State<VoteDialog> createState() => _VoteDialogState<T>();
}

class _VoteDialogState<T> extends State<VoteDialog<T>> {
  T? _decision;
  final _textController = TextEditingController();

  late final List<DropdownMenuItem<T>> _items = _getItems(widget.decisions);
  late final List<double> _customHeights = _getCustomItemHeights();

  @override
  void dispose() {
    _textController.dispose();
    super.dispose();
  }

  List<DropdownMenuItem<T>> _getItems(List<T> decisions) {
    var items = <DropdownMenuItem<T>>[];
    for (var decision in decisions) {
      items.addAll(
        [
          DropdownMenuItem<T>(
            value: decision,
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 8),
              child: Text(
                widget.getDisplayString(decision),
                style: GoogleFonts.raleway(
                  fontSize: 14,
                ),
              ),
            ),
          ),
          if (decision != decisions.last)
            DropdownMenuItem<T>(
              enabled: false,
              child: Divider(),
            ),
        ],
      );
    }

    return items;
  }

  List<double> _getCustomItemHeights() {
    var itemHeights = <double>[];
    for (int i = 0; i < _items.length; ++i) {
      itemHeights.add(i.isEven ? 40 : 4);
    }

    return itemHeights;
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      backgroundColor: Color(0xFF242423),
      title: Text(
        'Vote details',
        style: GoogleFonts.philosopher(),
      ),
      content: SizedBox(
        width: 350,
        height: 300,
        child: Column(
          children: [
            DropdownButton2<T>(
              isExpanded: true,
              hint: Text(
                'Your decision',
                style: GoogleFonts.raleway(
                  fontSize: 14,
                  color: Theme.of(context).hintColor,
                ),
              ),
              items: _items,
              customItemsHeights: _customHeights,
              value: _decision,
              onChanged: (value) => setState(() => _decision = value),
              buttonHeight: 40,
              dropdownMaxHeight: 200,
              itemPadding: const EdgeInsets.symmetric(horizontal: 8),
            ),
            SizedBox(height: 12),
            Expanded(
              child: TextField(
                controller: _textController,
                expands: true,
                maxLines: null,
                style: GoogleFonts.raleway(),
                decoration: InputDecoration(
                  hintText: 'Reason (optional). Be succinct.',
                  hintStyle: GoogleFonts.raleway(),
                  enabledBorder: UnderlineInputBorder(
                    borderSide: BorderSide(color: Colors.white70),
                  ),
                  focusedBorder: UnderlineInputBorder(
                    borderSide: BorderSide(color: Colors.white),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
      actions: [
        ElevatedButton(
          style: ElevatedButton.styleFrom(
            backgroundColor: Colors.white,
            foregroundColor: Colors.black,
          ),
          child: Text(
            'Vote',
            style: GoogleFonts.raleway(
              fontWeight: FontWeight.bold,
            ),
          ),
          onPressed: () async {
            if (_decision != null) {
              await widget.onVote(_decision!, _textController.text);
              Navigator.of(this.context).pop();
            }
          },
        ),
      ],
    );
  }
}
