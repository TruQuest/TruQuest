import 'package:flutter/material.dart';
import 'package:timelines/timelines.dart';

import '../models/rvm/verdict_vm.dart';
import '../../general/contexts/document_view_context.dart';
import '../../widget_extensions.dart';

class VerdictViewBlock extends StatelessWidgetX {
  late final _documentViewContext = useScoped<DocumentViewContext>();
  late final VerdictVm _verdict = _documentViewContext.proposal!.verdict;

  VerdictViewBlock({super.key});

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
              'Verdict',
              style: TextStyle(color: Colors.white),
            ),
          ),
        ),
        SizedBox(height: 6),
        SizedBox(
          width: double.infinity,
          height: 400,
          child: FixedTimeline.tileBuilder(
            theme: TimelineThemeData(
              nodePosition: 1,
            ),
            builder: TimelineTileBuilder.connected(
              itemCount: VerdictVm.values.length,
              contentsAlign: ContentsAlign.basic,
              connectionDirection: ConnectionDirection.before,
              indicatorBuilder: (_, index) => DotIndicator(
                color: _verdict.index == index
                    ? Colors.green[700]
                    : Colors.black54,
              ),
              oppositeContentsBuilder: (context, index) => Card(
                color:
                    _verdict.index == index ? Colors.green[700] : Colors.white,
                child: Padding(
                  padding: const EdgeInsets.all(12),
                  child: Text(
                    VerdictVm.values[index].getString(),
                    style: TextStyle(
                      color: _verdict.index == index
                          ? Colors.white
                          : Colors.black87,
                    ),
                  ),
                ),
              ),
              connectorBuilder: (context, index, type) {
                if (index == 0) {
                  return null;
                }

                if (_verdict.index == index) {
                  return DecoratedLineConnector(
                    decoration: BoxDecoration(
                      gradient: LinearGradient(
                        begin: Alignment.topCenter,
                        end: Alignment.bottomCenter,
                        colors: [
                          Colors.black54,
                          Colors.green[700]!,
                        ],
                      ),
                    ),
                  );
                } else if (_verdict.index == index - 1) {
                  return DecoratedLineConnector(
                    decoration: BoxDecoration(
                      gradient: LinearGradient(
                        begin: Alignment.topCenter,
                        end: Alignment.bottomCenter,
                        colors: [
                          Colors.green[700]!,
                          Colors.black54,
                        ],
                      ),
                    ),
                  );
                }

                return SolidLineConnector(color: Colors.black54);
              },
            ),
          ),
        ),
      ],
    );
  }
}
