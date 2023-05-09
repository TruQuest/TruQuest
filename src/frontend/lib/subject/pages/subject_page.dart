import 'package:flutter/material.dart';
import 'package:tab_container/tab_container.dart';

import '../widgets/things_list.dart';
import '../../general/widgets/document_view.dart';
import '../models/rvm/subject_vm.dart';
import '../widgets/avatar_with_reputation_gauge.dart';
import '../widgets/latest_things_block.dart';
import '../../general/contexts/document_view_context.dart';
import '../../general/widgets/arc_banner_image.dart';
import '../bloc/subject_actions.dart';
import '../bloc/subject_bloc.dart';
import '../../widget_extensions.dart';

class SubjectPage extends StatefulWidget {
  final String subjectId;

  SubjectPage({super.key, required this.subjectId});

  @override
  State<SubjectPage> createState() => _SubjectPageState();
}

class _SubjectPageState extends StateX<SubjectPage> {
  late final _subjectBloc = use<SubjectBloc>();

  @override
  void initState() {
    super.initState();
    _subjectBloc.dispatch(GetSubject(subjectId: widget.subjectId));
  }

  List<Widget> _buildTagChips(SubjectVm subject, TextTheme textTheme) {
    return subject.tags
        .map((tag) => Padding(
              padding: const EdgeInsets.only(right: 8),
              child: Chip(
                label: Text(tag.name),
                labelStyle: textTheme.caption,
                backgroundColor: Colors.black12,
              ),
            ))
        .toList();
  }

  Widget _buildHeader(SubjectVm subject) {
    var textTheme = Theme.of(context).textTheme;

    return Stack(
      children: [
        Padding(
          padding: const EdgeInsets.only(bottom: 100),
          child: ArcBannerImage(subject.imageIpfsCid),
        ),
        Positioned(
          bottom: 10,
          left: 40,
          right: 16,
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              AvatarWithReputationGauge(
                subjectId: subject.id,
                subjectAvatarIpfsCid: subject.croppedImageIpfsCid,
                size: AvatarSize.big,
                color: Colors.black87,
              ),
              SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      subject.name,
                      style: textTheme.titleLarge,
                    ),
                    SizedBox(height: 12),
                    Row(children: _buildTagChips(subject, textTheme)),
                  ],
                ),
              ),
              SizedBox(width: 16),
              Column(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  Text('Submitted at'),
                  SizedBox(height: 8),
                  Text(subject.submittedAtFormatted),
                  SizedBox(height: 8),
                  Text('by ${subject.submitterIdShort}'),
                ],
              ),
            ],
          ),
        ),
      ],
    );
  }

  List<Widget> _buildTabContents(SubjectVm subject) {
    var items = <Widget>[
      ScopeX(
        updatesShouldNotify: true,
        useInstances: [
          DocumentViewContext(
            nameOrTitle: subject.name,
            details: subject.details,
            tags: subject.tags.map((t) => t.name).toList(),
            subject: subject,
          ),
        ],
        child: DocumentView(
          sideBlocks: [
            LatestThingsBlock(),
          ],
        ),
      ),
      ThingsList(subjectId: subject.id),
    ];

    return items;
  }

  Widget _buildBody(SubjectVm subject) {
    var tabs = [
      Icon(Icons.content_paste),
      Icon(Icons.checklist_rtl),
    ];

    return SizedBox(
      width: double.infinity,
      height: 800,
      child: TabContainer(
        controller: TabContainerController(length: tabs.length),
        tabEdge: TabEdge.top,
        tabEnd: 0.3,
        color: Colors.blue[200],
        isStringTabs: false,
        tabs: tabs,
        children: _buildTabContents(subject),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return StreamBuilder(
      stream: _subjectBloc.subject$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return Center(child: CircularProgressIndicator());
        }

        var subject = snapshot.data!;

        return SingleChildScrollView(
          child: Column(
            children: [
              _buildHeader(subject),
              SizedBox(height: 30),
              _buildBody(subject),
            ],
          ),
        );
      },
    );
  }
}
