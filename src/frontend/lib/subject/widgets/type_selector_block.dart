import 'package:flutter/material.dart';

import '../../general/contexts/document_context.dart';
import '../models/im/subject_type_im.dart';
import '../../widget_extensions.dart';

class TypeSelectorBlock extends StatefulWidget {
  const TypeSelectorBlock({super.key});

  @override
  State<TypeSelectorBlock> createState() => _TypeSelectorBlockState();
}

class _TypeSelectorBlockState extends StateX<TypeSelectorBlock> {
  late final _documentContext = useScoped<DocumentContext>();

  @override
  Widget buildX(BuildContext context) {
    return Column(
      children: [
        Card(
          color: Colors.teal[600],
          elevation: 5,
          child: Container(
            width: double.infinity,
            height: 30,
            alignment: Alignment.center,
            child: Text(
              'Type',
              style: TextStyle(color: Colors.white),
            ),
          ),
        ),
        ListTile(
          title: Text('Person'),
          dense: true,
          leading: Radio<SubjectTypeIm>(
            value: SubjectTypeIm.person,
            groupValue: _documentContext.subjectType,
            onChanged: (value) {
              setState(() {
                _documentContext.subjectType = value;
              });
            },
          ),
        ),
        ListTile(
          title: Text('Organization'),
          dense: true,
          leading: Radio<SubjectTypeIm>(
            value: SubjectTypeIm.organization,
            groupValue: _documentContext.subjectType,
            onChanged: (value) {
              setState(() {
                _documentContext.subjectType = value;
              });
            },
          ),
        ),
      ],
    );
  }
}
