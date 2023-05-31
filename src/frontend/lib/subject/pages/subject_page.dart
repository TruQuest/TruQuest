import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:sliver_tools/sliver_tools.dart';

import '../models/rvm/subject_type_vm.dart';
import '../../general/widgets/tab_container.dart';
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

  List<Widget> _buildTagChips(SubjectVm subject) {
    return subject.tags
        .map(
          (tag) => Padding(
            padding: const EdgeInsets.only(right: 8),
            child: Chip(
              label: Text(tag.name),
              labelStyle: GoogleFonts.righteous(
                color: Color(0xffF8F9FA),
              ),
              backgroundColor: Color(0xFF242423),
            ),
          ),
        )
        .toList();
  }

  Widget _buildHeader(SubjectVm subject) {
    return Stack(
      children: [
        Padding(
          padding: const EdgeInsets.only(bottom: 100),
          child: ArcBannerImage(subject.imageIpfsCid),
        ),
        Positioned(
          bottom: 10,
          left: 100,
          right: 16,
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              AvatarWithReputationGauge(
                subjectId: subject.id,
                subjectAvatarIpfsCid: subject.croppedImageIpfsCid,
                value: subject.avgScore.toDouble(),
                size: AvatarSize.big,
                color: Color(0xFF242423),
              ),
              SizedBox(width: 32),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      subject.name,
                      style: GoogleFonts.philosopher(fontSize: 31),
                    ),
                    SizedBox(height: 12),
                    Row(children: _buildTagChips(subject)),
                  ],
                ),
              ),
              SizedBox(width: 16),
              Column(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  Text(
                    'Submitted on',
                    style: GoogleFonts.raleway(),
                  ),
                  SizedBox(height: 4),
                  Text(
                    subject.submittedAtFormatted,
                    style: GoogleFonts.raleway(
                      fontSize: 16,
                    ),
                  ),
                  SizedBox(height: 8),
                  RichText(
                    text: TextSpan(
                      children: [
                        TextSpan(
                          text: 'by ',
                          style: GoogleFonts.raleway(),
                        ),
                        TextSpan(
                          text: subject.submitterIdShort,
                          style: GoogleFonts.raleway(
                            fontSize: 16,
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ],
    );
  }

  List<Widget> _buildTabContents(SubjectVm subject) {
    return [
      ScopeX(
        updatesShouldNotify: true,
        useInstances: [
          DocumentViewContext(
            nameOrTitle: subject.name,
            details: subject.details,
            subject: subject,
          ),
        ],
        child: DocumentView(
          mainBlockMargin: const EdgeInsets.fromLTRB(26, 40, 0, 12),
          rightSideBlocks: [
            LatestThingsBlock(),
          ],
        ),
      ),
      ThingsList(subjectId: subject.id),
    ];
  }

  Widget _buildBody(SubjectVm subject) {
    var tabs = [
      Icon(
        Icons.content_paste,
        color: Colors.white,
      ),
      Icon(
        Icons.checklist_rtl,
        color: Colors.white,
      ),
    ];

    return SizedBox(
      width: double.infinity,
      height: 800,
      child: Stack(
        children: [
          TabContainer(
            controller: TabContainerController(length: tabs.length),
            tabEdge: TabEdge.top,
            tabStart: 0.33,
            tabEnd: 0.66,
            colors: [
              Color(0xFF242423),
              Color(0xFF413C69),
            ],
            isStringTabs: false,
            tabs: tabs,
            children: _buildTabContents(subject),
          ),
          Positioned(
            top: 28,
            left: 24,
            child: Card(
              margin: EdgeInsets.zero,
              color: Colors.blueAccent[400],
              child: Padding(
                padding: const EdgeInsets.fromLTRB(16, 8, 16, 8),
                child: Text(
                  subject.type.getString(),
                  style: GoogleFonts.righteous(
                    color: Colors.white,
                    fontSize: 22,
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return StreamBuilder(
      stream: _subjectBloc.subject$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return SliverFillRemaining(
            hasScrollBody: false,
            child: Center(child: CircularProgressIndicator()),
          );
        }

        var subject = snapshot.data!;

        return MultiSliver(
          children: [
            SliverToBoxAdapter(child: _buildHeader(subject)),
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.only(top: 30),
                child: _buildBody(subject),
              ),
            ),
          ],
        );
      },
    );
  }
}
