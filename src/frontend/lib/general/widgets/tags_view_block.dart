import 'package:flutter/material.dart';

import '../contexts/document_view_context.dart';
import '../../widget_extensions.dart';

class TagsViewBlock extends StatefulWidget {
  const TagsViewBlock({super.key});

  @override
  State<TagsViewBlock> createState() => _TagsViewBlockState();
}

class _TagsViewBlockState extends StateX<TagsViewBlock> {
  late final _documentViewContext = useScoped<DocumentViewContext>();

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Card(
          color: Colors.purple[800],
          elevation: 5,
          child: Container(
            width: double.infinity,
            height: 30,
            alignment: Alignment.center,
            child: Text(
              'Tags',
              style: TextStyle(color: Colors.white),
            ),
          ),
        ),
        SizedBox(height: 6),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 8),
          child: Wrap(
            spacing: 8,
            runSpacing: 8,
            children: _documentViewContext.tags
                .map(
                  (tag) => Card(
                    color: Colors.white,
                    shape: RoundedRectangleBorder(
                      side: BorderSide(color: Colors.purple[800]!),
                    ),
                    elevation: 5,
                    child: Padding(
                      padding: EdgeInsets.fromLTRB(12, 8, 12, 8),
                      child: Text(
                        tag,
                        overflow: TextOverflow.fade,
                        softWrap: false,
                        style: TextStyle(
                          color: Colors.purple[800],
                          fontSize: 12,
                        ),
                      ),
                    ),
                  ),
                )
                .toList(),
          ),
        ),
      ],
    );
  }
}
