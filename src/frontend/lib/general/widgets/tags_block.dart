import 'package:flutter/material.dart';
import 'package:multi_select_flutter/multi_select_flutter.dart';

import '../models/rvm/tag_vm.dart';
import '../bloc/general_bloc.dart';
import '../bloc/general_actions.dart';
import '../contexts/document_context.dart';
import '../../widget_extensions.dart';
import '../models/im/tag_im.dart';

class TagsBlock extends StatefulWidget {
  const TagsBlock({super.key});

  @override
  State<TagsBlock> createState() => _TagsBlockState();
}

class _TagsBlockState extends StateX<TagsBlock> {
  late final _generalBloc = use<GeneralBloc>();
  late final _documentContext = useScoped<DocumentContext>();

  late final Future<List<TagVm>?> _tagsFuture;

  @override
  void initState() {
    super.initState();
    _tagsFuture = _generalBloc.execute<List<TagVm>>(const GetTags());
  }

  @override
  Widget buildX(BuildContext context) {
    return FutureBuilder(
      future: _tagsFuture,
      builder: (context, snapshot) {
        List<MultiSelectItem<int>>? items;
        var tags = snapshot.data;
        if (tags != null) {
          items = tags.map((t) => MultiSelectItem(t.id, t.name)).toList();
        }

        return Container(
          decoration: const BoxDecoration(
            border: Border.symmetric(
              horizontal: BorderSide(
                color: Colors.blue,
                width: 2,
              ),
            ),
          ),
          padding: const EdgeInsets.only(bottom: 4),
          child: MultiSelectDialogField<int>(
            decoration: const BoxDecoration(
              border: Border(
                bottom: BorderSide(
                  color: Colors.transparent,
                  width: 2,
                ),
              ),
            ),
            buttonText: const Text('Tags'),
            dialogWidth: 400,
            items: items ?? [],
            listType: MultiSelectListType.CHIP,
            onConfirm: (tagIds) => _documentContext.tags.addAll(
              tagIds.map((tagId) => TagIm(id: tagId)),
            ),
          ),
        );
      },
    );
  }
}
