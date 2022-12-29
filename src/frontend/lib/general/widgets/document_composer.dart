// ignore_for_file: prefer_const_constructors

import 'package:flutter/material.dart';
import 'package:flutter_quill/flutter_quill.dart' as quill;

class DocumentComposer extends StatefulWidget {
  final List<Widget> sideBlocks;

  const DocumentComposer({super.key, required this.sideBlocks});

  @override
  State<DocumentComposer> createState() => _DocumentComposerState();
}

class _DocumentComposerState extends State<DocumentComposer> {
  final quill.QuillController _controller = quill.QuillController.basic();

  @override
  Widget build(BuildContext context) {
    return SimpleDialog(
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.all(Radius.circular(12)),
      ),
      titlePadding: const EdgeInsets.fromLTRB(12, 12, 12, 0),
      title: SizedBox(
        width: 800,
        height: 30,
        child: Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text("Subject"),
            Row(
              children: [
                IconButton(
                  padding: EdgeInsets.zero,
                  icon: Icon(Icons.preview_outlined),
                  iconSize: 20,
                  splashRadius: 13,
                  color: Colors.blue[900],
                  onPressed: () {},
                ),
                IconButton(
                  padding: EdgeInsets.zero,
                  icon: Icon(Icons.close),
                  iconSize: 20,
                  splashRadius: 13,
                  color: Colors.red[800],
                  onPressed: () {},
                ),
              ],
            ),
          ],
        ),
      ),
      contentPadding: const EdgeInsets.fromLTRB(12, 16, 12, 8),
      children: [
        SizedBox(
          width: 800,
          height: 600,
          child: Column(
            children: [
              Expanded(
                child: quill.QuillToolbar.basic(
                  controller: _controller,
                  toolbarIconAlignment: WrapAlignment.start,
                  showCodeBlock: false,
                  showSearchButton: false,
                ),
              ),
              Expanded(
                flex: 6,
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Expanded(
                      flex: 3,
                      child: Container(
                        margin: const EdgeInsets.only(top: 8),
                        decoration: BoxDecoration(
                          border: Border.all(
                            color: Colors.black.withOpacity(0.3),
                          ),
                          borderRadius: BorderRadius.all(Radius.circular(12)),
                        ),
                        padding: const EdgeInsets.all(4),
                        child: quill.QuillEditor(
                          controller: _controller,
                          placeholder: "Fill in the details...",
                          scrollController: ScrollController(),
                          scrollable: true,
                          focusNode: FocusNode(),
                          autoFocus: true,
                          readOnly: false,
                          expands: true,
                          padding: const EdgeInsets.all(12),
                        ),
                      ),
                    ),
                    Expanded(
                      child: SingleChildScrollView(
                        child: Column(
                          children: widget.sideBlocks
                              .map(
                                (block) => Padding(
                                  padding: const EdgeInsets.symmetric(
                                    horizontal: 8,
                                    vertical: 4,
                                  ),
                                  child: block,
                                ),
                              )
                              .toList(),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              Expanded(
                child: Row(
                  children: [
                    Flexible(
                      flex: 3,
                      child: TextField(
                        maxLength: 50,
                        decoration: InputDecoration(
                          labelText: "Name",
                        ),
                      ),
                    ),
                    Expanded(
                      child: Center(
                        child: ElevatedButton(
                          style: ElevatedButton.styleFrom(
                            backgroundColor: Colors.amber[900],
                            elevation: 8,
                            padding: const EdgeInsets.symmetric(
                              horizontal: 36,
                              vertical: 16,
                            ),
                          ),
                          child: Text("Submit"),
                          onPressed: () {},
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
