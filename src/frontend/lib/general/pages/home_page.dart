// ignore_for_file: prefer_const_constructors, sort_child_properties_last

import "package:flutter/material.dart";

import '../widgets/image_block.dart';
import '../widgets/tags_block.dart';
import '../widgets/document_composer.dart';
import '../widgets/evidence_block.dart';
import "../widgets/status_panel.dart";

class HomePage extends StatelessWidget {
  const HomePage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text("TruQuest"),
        actions: [
          StatusPanel(),
        ],
      ),
      body: Center(
        child: Text("Hello!"),
      ),
      floatingActionButton: FloatingActionButton(
        child: Icon(Icons.edit),
        onPressed: () {
          showDialog(
            context: context,
            barrierDismissible: false,
            builder: (_) => DocumentComposer(
              sideBlocks: [
                ImageBlock(),
                TagsBlock(),
                EvidenceBlock(),
              ],
            ),
          );
        },
      ),
    );
  }
}