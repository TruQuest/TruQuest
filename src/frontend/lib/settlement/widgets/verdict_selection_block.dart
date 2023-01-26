import 'package:flutter/material.dart';
import 'package:dropdown_button2/dropdown_button2.dart';

import '../../general/contexts/document_context.dart';
import '../models/im/verdict_im.dart';
import '../../widget_extensions.dart';

class VerdictSelectionBlock extends StatefulWidget {
  const VerdictSelectionBlock({super.key});

  @override
  State<VerdictSelectionBlock> createState() => _VerdictSelectionBlockState();
}

class _VerdictSelectionBlockState extends StateX<VerdictSelectionBlock> {
  late final _documentContext = useScoped<DocumentContext>();

  late final List<DropdownMenuItem<VerdictIm>> _items =
      _getItems(VerdictIm.values);
  late final List<double> _customHeights = _getCustomItemHeights();

  List<DropdownMenuItem<VerdictIm>> _getItems(List<VerdictIm> verdicts) {
    var items = <DropdownMenuItem<VerdictIm>>[];
    for (var verdict in verdicts) {
      items.addAll(
        [
          DropdownMenuItem<VerdictIm>(
            value: verdict,
            child: Padding(
              padding: EdgeInsets.symmetric(horizontal: 8),
              child: Text(
                verdict.getString(),
                style: TextStyle(
                  fontSize: 14,
                ),
              ),
            ),
          ),
          if (verdict != verdicts.last)
            DropdownMenuItem<VerdictIm>(
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
    return Column(
      children: [
        Card(
          color: Colors.teal[600],
          elevation: 5,
          child: Container(
            width: double.infinity,
            height: 30,
            alignment: Alignment.center,
            child: Text(
              'Verdict',
              style: TextStyle(color: Colors.white),
            ),
          ),
        ),
        SizedBox(height: 6),
        DropdownButtonHideUnderline(
          child: DropdownButton2<VerdictIm>(
            isExpanded: true,
            hint: Text(
              'Your verdict',
              style: TextStyle(
                fontSize: 14,
                color: Theme.of(context).hintColor,
              ),
            ),
            items: _items,
            customItemsHeights: _customHeights,
            value: _documentContext.verdict,
            onChanged: (value) =>
                setState(() => _documentContext.verdict = value),
            buttonHeight: 40,
            dropdownMaxHeight: 200,
            itemPadding: EdgeInsets.symmetric(horizontal: 8),
          ),
        ),
      ],
    );
  }
}
