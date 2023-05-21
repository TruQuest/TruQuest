import 'package:flutter/material.dart';
import 'package:flutter_quill/flutter_quill.dart' hide Text;
import 'package:google_fonts/google_fonts.dart';

import '../contexts/document_context.dart';
import '../../widget_extensions.dart';

class DocumentComposer extends StatefulWidget {
  final String title;
  final String nameFieldLabel;
  final Widget submitButton;
  final List<Widget> sideBlocks;

  const DocumentComposer({
    super.key,
    required this.title,
    required this.nameFieldLabel,
    required this.submitButton,
    required this.sideBlocks,
  });

  @override
  State<DocumentComposer> createState() => _DocumentComposerState();
}

class _DocumentComposerState extends StateX<DocumentComposer> {
  late final _documentContext = useScoped<DocumentContext>();

  late final QuillController _controller = _documentContext.quillController!;
  final _scrollController = ScrollController();
  final _focusNode = FocusNode();

  @override
  void dispose() {
    _scrollController.dispose();
    _focusNode.dispose();
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return SimpleDialog(
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
      ),
      titlePadding: const EdgeInsets.fromLTRB(12, 12, 12, 0),
      title: SizedBox(
        width: 800,
        height: 30,
        child: Row(
          children: [
            Text(
              widget.title,
              style: GoogleFonts.righteous(
                color: Colors.black87,
              ),
            ),
            Spacer(),
            IconButton(
              padding: EdgeInsets.zero,
              icon: Icon(Icons.help_outline),
              iconSize: 20,
              splashRadius: 13,
              color: Colors.blue[900],
              onPressed: () {},
            ),
            SizedBox(width: 20),
            IconButton(
              padding: EdgeInsets.zero,
              icon: Icon(Icons.close),
              iconSize: 20,
              splashRadius: 13,
              color: Colors.red[800],
              onPressed: () {
                Navigator.of(context).pop();
              },
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
                child: QuillToolbar.basic(
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
                          borderRadius: BorderRadius.circular(12),
                        ),
                        padding: const EdgeInsets.all(4),
                        child: QuillEditor(
                          controller: _controller,
                          placeholder: 'Fill in the details...',
                          scrollController: _scrollController,
                          scrollable: true,
                          focusNode: _focusNode,
                          autoFocus: false,
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
                        autofocus: true,
                        maxLength: 50,
                        decoration: InputDecoration(
                          labelText: widget.nameFieldLabel,
                        ),
                        onChanged: (value) {
                          _documentContext.nameOrTitle = value;
                        },
                      ),
                    ),
                    Expanded(child: Center(child: widget.submitButton)),
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
