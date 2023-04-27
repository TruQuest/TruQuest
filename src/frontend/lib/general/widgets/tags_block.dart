import 'package:flutter/material.dart';

import '../contexts/document_context.dart';
import '../../widget_extensions.dart';

class TagsBlock extends StatefulWidget {
  const TagsBlock({super.key});

  @override
  State<TagsBlock> createState() => _TagsBlockState();
}

class _TagsBlockState extends StateX<TagsBlock> {
  late final _documentContext = useScoped<DocumentContext>();

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        OutlinedButton(
          style: OutlinedButton.styleFrom(
            backgroundColor: Colors.purple[800],
            foregroundColor: Colors.white,
            elevation: 5,
          ),
          child: Row(
            children: [
              Spacer(),
              Text('Tags'),
              Expanded(
                child: Align(
                  alignment: Alignment.centerRight,
                  child: Icon(Icons.add),
                ),
              ),
            ],
          ),
          onPressed: () {
            showDialog(
              context: context,
              builder: (_) => AlertDialog(
                title: Text('asdasd'),
                content: Center(
                  child: TextButton(
                    child: Text('Add'),
                    onPressed: () {
                      setState(() {
                        _documentContext.tags.add('tag1');
                      });
                    },
                  ),
                ),
              ),
            );
          },
        ),
        SizedBox(height: 6),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 8),
          child: Wrap(
            spacing: 8,
            runSpacing: 8,
            children: _documentContext.tags
                .map(
                  (tag) => OutlinedButton(
                    style: OutlinedButton.styleFrom(
                      foregroundColor: Colors.purple[800],
                      side: BorderSide(color: Colors.purple[800]!),
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
                            tag,
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
                )
                .toList(),
          ),
        ),
      ],
    );
  }
}
