import 'package:flutter/material.dart';

import '../../general/contexts/document_view_context.dart';
import '../../widget_extensions.dart';

class LatestThingsBlock extends StatefulWidget {
  const LatestThingsBlock({super.key});

  @override
  State<LatestThingsBlock> createState() => _LatestThingsBlockState();
}

class _LatestThingsBlockState extends StateX<LatestThingsBlock> {
  late DocumentViewContext _documentViewContext;

  final Map<int, bool> _isExpanded = {0: false, 1: false};

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    _documentViewContext = useScoped<DocumentViewContext>();
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Card(
          color: Colors.deepPurpleAccent[400],
          margin: EdgeInsets.zero,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.only(
              topLeft: Radius.circular(12),
              topRight: Radius.circular(12),
            ),
          ),
          elevation: 5,
          child: Container(
            width: double.infinity,
            height: 30,
            alignment: Alignment.center,
            child: Text(
              'Latest',
              style: TextStyle(color: Colors.white),
            ),
          ),
        ),
        ExpansionPanelList(
          expansionCallback: (index, isExpanded) => setState(() {
            _isExpanded[index] = !isExpanded;
          }),
          dividerColor: Colors.purple[900],
          children: [
            ExpansionPanel(
              isExpanded: _isExpanded[0]!,
              canTapOnHeader: true,
              headerBuilder: (_, __) => ListTile(
                title: Text('Settled'),
              ),
              body: _documentViewContext.subject!.latestSettledThings.isNotEmpty
                  ? Column(
                      children:
                          _documentViewContext.subject!.latestSettledThings
                              .map(
                                (thing) => Padding(
                                  padding: const EdgeInsets.only(bottom: 8),
                                  child: Text(thing.title),
                                ),
                              )
                              .toList(),
                    )
                  : Container(
                      color: Colors.yellow[300],
                      width: double.infinity,
                      height: 40,
                      alignment: Alignment.center,
                      child: Text('Nothing here yet'),
                    ),
            ),
            ExpansionPanel(
              isExpanded: _isExpanded[1]!,
              canTapOnHeader: true,
              headerBuilder: (_, __) => ListTile(
                title: Text('Unsettled'),
              ),
              body: _documentViewContext
                      .subject!.latestUnsettledThings.isNotEmpty
                  ? Column(
                      children:
                          _documentViewContext.subject!.latestUnsettledThings
                              .map(
                                (thing) => Padding(
                                  padding: const EdgeInsets.only(bottom: 8),
                                  child: Text(thing.title),
                                ),
                              )
                              .toList(),
                    )
                  : Container(
                      color: Colors.yellow[300],
                      width: double.infinity,
                      height: 40,
                      alignment: Alignment.center,
                      child: Text('Nothing here yet'),
                    ),
            ),
          ],
        ),
      ],
    );
  }
}
