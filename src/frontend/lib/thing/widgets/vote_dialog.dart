import 'dart:async';

import 'package:flutter/material.dart';
import 'package:dropdown_button2/dropdown_button2.dart';

import '../models/im/decision_im.dart';

class VoteDialog extends StatefulWidget {
  final FutureOr<void> Function(DecisionIm decision, String reason) onVote;

  const VoteDialog({super.key, required this.onVote});

  @override
  State<VoteDialog> createState() => _VoteDialogState();
}

class _VoteDialogState extends State<VoteDialog> {
  DecisionIm? _decision;
  final _textController = TextEditingController();

  final List<DecisionIm> _decisions = [
    DecisionIm.accept,
    DecisionIm.softDecline,
    DecisionIm.hardDecline,
  ];

  late final List<DropdownMenuItem<DecisionIm>> _items = _getItems(_decisions);
  late final List<double> _customHeights = _getCustomItemHeights();

  @override
  void dispose() {
    _textController.dispose();
    super.dispose();
  }

  List<DropdownMenuItem<DecisionIm>> _getItems(List<DecisionIm> decisions) {
    var items = <DropdownMenuItem<DecisionIm>>[];
    for (var decision in decisions) {
      items.addAll(
        [
          DropdownMenuItem<DecisionIm>(
            value: decision,
            child: Padding(
              padding: EdgeInsets.symmetric(horizontal: 8),
              child: Text(
                decision.getString(),
                style: TextStyle(
                  fontSize: 14,
                ),
              ),
            ),
          ),
          if (decision != decisions.last)
            DropdownMenuItem<DecisionIm>(
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
              child: DropdownButton2<DecisionIm>(
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
                onChanged: (value) {
                  setState(() => _decision = value);
                },
                buttonHeight: 40,
                dropdownMaxHeight: 200,
                // buttonWidth: 140,
                itemPadding: EdgeInsets.symmetric(horizontal: 8),
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
