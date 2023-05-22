import 'package:flutter/material.dart';

import '../contexts/document_context.dart';
import '../../widget_extensions.dart';

class EvidenceBlock extends StatefulWidget {
  const EvidenceBlock({super.key});

  @override
  State<EvidenceBlock> createState() => _EvidenceBlockState();
}

class _EvidenceBlockState extends StateX<EvidenceBlock> {
  late final _documentContext = useScoped<DocumentContext>();

  final _textController = TextEditingController();

  @override
  void dispose() {
    _textController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        OutlinedButton(
          style: OutlinedButton.styleFrom(
            backgroundColor: Colors.blue[800],
            foregroundColor: Colors.white,
            elevation: 5,
          ),
          child: Row(
            children: [
              Spacer(),
              Text('Evidence'),
              Expanded(
                child: Align(
                  alignment: Alignment.centerRight,
                  child: Icon(Icons.add),
                ),
              ),
            ],
          ),
          onPressed: () async {
            var ok = await showDialog<bool>(
              context: context,
              builder: (_) => AlertDialog(
                title: Text('Add evidence'),
                content: Container(
                  width: 400,
                  height: 60,
                  alignment: Alignment.center,
                  child: TextField(
                    controller: _textController,
                    decoration: InputDecoration(
                      hintText: 'Paste url here',
                    ),
                    onSubmitted: (_) => Navigator.of(context).pop(true),
                  ),
                ),
                actions: [
                  TextButton(
                    style: TextButton.styleFrom(
                      backgroundColor: Color(0xFF242423),
                      foregroundColor: Colors.white,
                    ),
                    child: Text('Ok'),
                    onPressed: () {
                      Navigator.of(context).pop(true);
                    },
                  )
                ],
              ),
            );

            if (ok != null && ok && _textController.text.isNotEmpty) {
              setState(() {
                _documentContext.evidence.add(_textController.text);
                _textController.clear();
              });
            }
          },
        ),
        SizedBox(height: 6),
        ..._documentContext.evidence.map(
          (link) => Padding(
            padding: const EdgeInsets.fromLTRB(8, 4, 8, 4),
            child: OutlinedButton(
              style: OutlinedButton.styleFrom(
                foregroundColor: Colors.blue[800],
                side: BorderSide(color: Colors.blue[800]!),
                padding: const EdgeInsets.fromLTRB(4, 0, 8, 0),
                minimumSize: Size(0, 36),
              ),
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(
                    Icons.delete_outline,
                    size: 18,
                  ),
                  SizedBox(width: 2),
                  Flexible(
                    child: Text(
                      link,
                      overflow: TextOverflow.fade,
                      softWrap: false,
                      textAlign: TextAlign.center,
                      style: TextStyle(fontSize: 12),
                    ),
                  ),
                ],
              ),
              onPressed: () {},
            ),
          ),
        ),
      ],
    );
  }
}
