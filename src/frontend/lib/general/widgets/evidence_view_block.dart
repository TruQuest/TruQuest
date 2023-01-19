import 'package:flutter/material.dart';
import 'package:fading_edge_scrollview/fading_edge_scrollview.dart';

import '../../widget_extensions.dart';

class EvidenceViewBlock extends StatefulWidget {
  const EvidenceViewBlock({super.key});

  @override
  State<EvidenceViewBlock> createState() => _EvidenceViewBlockState();
}

class _EvidenceViewBlockState extends StateX<EvidenceViewBlock> {
  final _scrollController = ScrollController();

  @override
  void dispose() {
    _scrollController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Container(
          color: Colors.blue[600],
          width: double.infinity,
          height: 30,
          alignment: Alignment.centerLeft,
          child: Text(
            'Evidence',
            style: TextStyle(color: Colors.white),
          ),
        ),
        Expanded(
          child: FadingEdgeScrollView.fromScrollView(
            child: ListView(
              controller: _scrollController,
              scrollDirection: Axis.horizontal,
              children: [
                'https://cdn.pixabay.com/photo/2020/04/11/08/14/market-5029331_1280.jpg',
                'https://cdn.pixabay.com/photo/2020/04/11/08/14/market-5029331_1280.jpg',
                'https://cdn.pixabay.com/photo/2020/04/11/08/14/market-5029331_1280.jpg',
                'https://cdn.pixabay.com/photo/2020/04/11/08/14/market-5029331_1280.jpg',
                'https://cdn.pixabay.com/photo/2020/04/11/08/14/market-5029331_1280.jpg',
                'https://cdn.pixabay.com/photo/2020/04/11/08/14/market-5029331_1280.jpg',
                'https://cdn.pixabay.com/photo/2020/04/11/08/14/market-5029331_1280.jpg',
                'https://cdn.pixabay.com/photo/2020/04/11/08/14/market-5029331_1280.jpg',
                'https://cdn.pixabay.com/photo/2020/04/11/08/14/market-5029331_1280.jpg',
              ]
                  .map(
                    (url) => Card(
                      margin: EdgeInsets.fromLTRB(6, 6, 6, 6),
                      elevation: 5,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.all(Radius.circular(12)),
                      ),
                      clipBehavior: Clip.antiAlias,
                      child: Stack(
                        children: [
                          AspectRatio(
                            aspectRatio: 16 / 9,
                            child: Image.network(
                              url,
                              fit: BoxFit.cover,
                            ),
                          ),
                          Positioned(
                            bottom: 0,
                            left: 0,
                            right: 0,
                            child: Container(
                              color: Colors.black54,
                              padding: EdgeInsets.symmetric(
                                horizontal: 8,
                                vertical: 8,
                              ),
                              alignment: Alignment.center,
                              child: Text(
                                url,
                                overflow: TextOverflow.fade,
                                softWrap: false,
                                style: TextStyle(
                                  color: Colors.white,
                                ),
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  )
                  .toList(),
            ),
          ),
        ),
      ],
    );
  }
}
