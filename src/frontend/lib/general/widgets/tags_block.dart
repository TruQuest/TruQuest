// ignore_for_file: sort_child_properties_last, prefer_const_constructors

import 'package:flutter/material.dart';

class TagsBlock extends StatefulWidget {
  const TagsBlock({super.key});

  @override
  State<TagsBlock> createState() => _TagsBlockState();
}

class _TagsBlockState extends State<TagsBlock> {
  final List<String> _tags = ["Politics", "Sport", "IT", "Entertainment"];

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
              Text("Tags"),
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
              barrierDismissible: true,
              builder: (_) => AlertDialog(
                title: Text("asdasd"),
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
            children: _tags
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
