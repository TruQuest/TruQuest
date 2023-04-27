import 'dart:async';

import 'package:flutter/material.dart';
import 'package:dropdown_button2/dropdown_button2.dart';

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
                style: TextStyle(
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
      title: Text('Vote details'),
      content: SizedBox(
        width: 350,
        height: 500,
        child: Column(
          children: [
            DropdownButtonHideUnderline(
              child: DropdownButton2<T>(
                isExpanded: true,
                hint: Text(
                  'Your decision',
                  style: TextStyle(
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
            ),
            SizedBox(height: 12),
            Expanded(
              child: TextField(
                controller: _textController,
                expands: true,
                maxLines: null,
                decoration: InputDecoration(
                  hintText: 'Reason (optional). Be succinct',
                ),
              ),
            ),
          ],
        ),
      ),
      actions: [
        TextButton(
          child: Text('Vote'),
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
