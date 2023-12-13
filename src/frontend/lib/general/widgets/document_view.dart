import 'package:flutter/material.dart';
import 'package:flutter_quill/flutter_quill.dart' hide Text;

import '../contexts/document_view_context.dart';
import '../../widget_extensions.dart';

class DocumentView extends StatefulWidget {
  final EdgeInsets mainBlockMargin;
  final List<Widget> rightSideBlocks;
  final Widget? leftSideBlock;

  const DocumentView({
    super.key,
    this.mainBlockMargin = const EdgeInsets.fromLTRB(0, 32, 0, 12),
    required this.rightSideBlocks,
    this.leftSideBlock,
  });

  @override
  State<DocumentView> createState() => _DocumentViewState();
}

class _DocumentViewState extends StateX<DocumentView> {
  late DocumentViewContext _documentViewContext;

  late QuillController _controller;
  final _scrollController = ScrollController();
  final _focusNode = FocusNode();
  final _sideBlocksScrollController = ScrollController();

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    _documentViewContext = useScoped<DocumentViewContext>();
    _controller = _documentViewContext.quillController;
  }

  @override
  void dispose() {
    _scrollController.dispose();
    _focusNode.dispose();
    _sideBlocksScrollController.dispose();
    super.dispose();
  }

  @override
  Widget buildX(BuildContext context) {
    return Container(
      width: double.infinity,
      height: double.infinity,
      decoration: const BoxDecoration(
        color: Color(0xFF242423),
        borderRadius: BorderRadius.only(
          topLeft: Radius.circular(16),
          topRight: Radius.circular(16),
        ),
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          if (widget.leftSideBlock != null) Expanded(flex: 17, child: widget.leftSideBlock!),
          Expanded(
            flex: widget.leftSideBlock != null ? 59 : 70,
            child: Container(
              margin: widget.mainBlockMargin,
              decoration: BoxDecoration(
                color: const Color(0xffF8F9FA),
                borderRadius: BorderRadius.circular(12),
              ),
              padding: const EdgeInsets.all(4),
              child: QuillEditor(
                key: const PageStorageKey('quill'),
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
            flex: 24,
            child: SingleChildScrollView(
              key: const PageStorageKey('side-blocks'),
              controller: _sideBlocksScrollController,
              child: Column(
                children: widget.rightSideBlocks
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
