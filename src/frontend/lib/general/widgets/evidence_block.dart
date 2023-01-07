// ignore_for_file: sort_child_properties_last, prefer_const_constructors

import 'package:flutter/material.dart';

class EvidenceBlock extends StatefulWidget {
  const EvidenceBlock({super.key});

  @override
  State<EvidenceBlock> createState() => _EvidenceBlockState();
}

class _EvidenceBlockState extends State<EvidenceBlock> {
  final List<String> _links = [
    "http://sports.ru/",
    "https://media.istockphoto.com/id/1214625216/photo/elephant-with-a-zebra-skin-walking-in-savannah-this-is-a-3d-render-illustration.jpg?b=1&s=170667a&w=0&k=20&c=rRpDFrSK4uVCq73R2AaevGfq5RkqSq0MRZZ_RnYxLX8=",
    "http://www.stackoverflow.com/",
  ];

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        OutlinedButton(
          style: OutlinedButton.styleFrom(
            backgroundColor: Colors.blue[800],
            foregroundColor: Colors.white,
            elevation: 5,
          ),
          child: Row(
            children: [
              Spacer(),
              Text("Evidence"),
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
        ..._links.map(
          (link) => Padding(
            padding: const EdgeInsets.fromLTRB(8, 4, 8, 4),
            child: OutlinedButton(
              style: OutlinedButton.styleFrom(
                foregroundColor: Colors.blue[800],
                side: BorderSide(color: Colors.blue[800]!),
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
                      link,
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
          ),
        ),
      ],
    );
  }
}
