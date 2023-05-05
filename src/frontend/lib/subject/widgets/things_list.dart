import 'package:flutter/material.dart';
import 'package:auto_size_text/auto_size_text.dart';

import 'corner_banner.dart';
import '../bloc/subject_actions.dart';
import '../bloc/subject_bloc.dart';
import '../../widget_extensions.dart';
import 'clipped_image.dart';

class ThingsList extends StatefulWidget {
  final String subjectId;

  const ThingsList({super.key, required this.subjectId});

  @override
  State<ThingsList> createState() => _ThingsListState();
}

class _ThingsListState extends StateX<ThingsList> {
  late final _subjectBloc = use<SubjectBloc>();

  @override
  void initState() {
    super.initState();
    _subjectBloc.dispatch(GetThingsList(subjectId: widget.subjectId));
  }

  @override
  Widget build(BuildContext context) {
    return StreamBuilder(
      stream: _subjectBloc.thingsList$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return Center(child: CircularProgressIndicator());
        }

        var things = snapshot.data!.things;
        if (things.isEmpty) {
          return Center(child: Text('Nothing here yet'));
        }

        return Column(
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                IconButton(
                  icon: Icon(Icons.add_box_outlined),
                  onPressed: () {},
                ),
              ],
            ),
            ListView.builder(
              shrinkWrap: true,
              padding: const EdgeInsets.all(16),
              itemCount: things.length,
              itemBuilder: (context, index) {
                var thing = things[index];
                return Stack(
                  children: [
                    Card(
                      margin: EdgeInsets.zero,
                      color: Colors.blue[600],
                      elevation: 5,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Padding(
                        padding: const EdgeInsets.only(left: 250),
                        child: SizedBox(
                          width: 500,
                          height: 135,
                          child: Row(
                            children: [
                              Expanded(
                                child: Column(
                                  mainAxisAlignment: MainAxisAlignment.center,
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    AutoSizeText(
                                      thing.title,
                                      style: TextStyle(
                                        color: Colors.white,
                                        fontSize: 18,
                                      ),
                                      maxLines: 2,
                                      overflow: TextOverflow.ellipsis,
                                    ),
                                    SizedBox(height: 12),
                                    Text(
                                      thing.displayedTimestampFormatted,
                                      style: TextStyle(
                                        color: Colors.white70,
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                              SizedBox(width: 12),
                              InkWell(
                                borderRadius: BorderRadius.only(
                                  topRight: Radius.circular(12),
                                  bottomRight: Radius.circular(12),
                                ),
                                child: Container(
                                  width: 42,
                                  height: double.infinity,
                                  decoration: BoxDecoration(
                                    color: Colors.grey[400],
                                    borderRadius: BorderRadius.only(
                                      topRight: Radius.circular(12),
                                      bottomRight: Radius.circular(12),
                                    ),
                                  ),
                                  alignment: Alignment.center,
                                  child: Icon(
                                    Icons.arrow_forward_ios_rounded,
                                    color: Colors.white,
                                  ),
                                ),
                                onTap: () {},
                              ),
                            ],
                          ),
                        ),
                      ),
                    ),
                    ClippedImage(
                      imageIpfsCid: thing.croppedImageIpfsCid!,
                      width: 240,
                      height: 135,
                      // fromNarrowToWide: true,
                    ),
                    CornerBanner(
                      position: Alignment.topLeft,
                      size: 50,
                      cornerRadius: 12,
                      color: thing.verdict != null
                          ? thing.verdictColor
                          : Colors.grey[400]!,
                      icon: thing.stateIcon,
                      iconSize: 22,
                    )
                  ],
                );
              },
            ),
          ],
        );
      },
    );
  }
}
