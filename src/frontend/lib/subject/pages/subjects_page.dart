import 'package:flutter/material.dart';

import '../../general/contexts/page_context.dart';
import '../widgets/submit_button.dart';
import '../widgets/type_selector_block.dart';
import '../../general/widgets/document_composer.dart';
import '../../general/widgets/image_block_with_crop.dart';
import '../../general/widgets/tags_block.dart';
import '../../widget_extensions.dart';

class SubjectsPage extends StatelessWidgetX {
  late final _pageContext = useScoped<PageContext>();

  SubjectsPage({super.key});

  @override
  Widget buildX(BuildContext context) {
    return Scaffold(
      body: Center(child: Text('Subjects')),
      floatingActionButton: FloatingActionButton(
        child: Icon(Icons.add),
        onPressed: () async {
          var jumpToRoute = await showDialog<String>(
            context: context,
            barrierDismissible: false,
            builder: (_) => ScopeX(
              child: DocumentComposer(
                title: 'New subject',
                nameFieldLabel: 'Name',
                submitButton: SubmitButton(),
                sideBlocks: [
                  TypeSelectorBlock(),
                  ImageBlockWithCrop(cropCircle: true),
                  TagsBlock(),
                ],
              ),
            ),
          );

          if (jumpToRoute != null) {
            _pageContext.route = jumpToRoute;
            _pageContext.controller.jumpToPage(
              DateTime.now().millisecondsSinceEpoch,
            );
          }
        },
      ),
    );
  }
}
