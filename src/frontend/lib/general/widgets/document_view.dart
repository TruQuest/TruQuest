import 'package:flutter/material.dart';
import 'package:flutter_quill/flutter_quill.dart';

import '../contexts/document_view_context.dart';
import '../../widget_extensions.dart';

class DocumentView extends StatefulWidget {
  final List<Widget> sideBlocks;

  const DocumentView({
    super.key,
    required this.sideBlocks,
  });

  @override
  State<DocumentView> createState() => _DocumentViewState();
}

class _DocumentViewState extends StateX<DocumentView> {
  late final _documentViewContext = useScoped<DocumentViewContext>();

  late final QuillController _controller = _documentViewContext.quillController;
  final _scrollController = ScrollController();
  final _focusNode = FocusNode();

  @override
  void dispose() {
    _scrollController.dispose();
    _focusNode.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return SizedBox.expand(
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            flex: 3,
            child: Padding(
              padding: const EdgeInsets.all(4),
              child: QuillEditor(
                key: PageStorageKey('quill'),
                controller: _controller,
                scrollController: _scrollController,
                scrollable: true,
                focusNode: _focusNode,
                autoFocus: false,
                readOnly: true,
                expands: true,
                showCursor: false,
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
    );
  }
}
