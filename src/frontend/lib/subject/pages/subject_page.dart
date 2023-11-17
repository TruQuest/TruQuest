import 'package:flutter/material.dart';
import 'package:contained_tab_bar_view/contained_tab_bar_view.dart'
    show TabBarProperties, TabBarViewProperties, ContainerTabIndicator;
import 'package:google_fonts/google_fonts.dart';
import 'package:sliver_tools/sliver_tools.dart';

import '../../general/widgets/contained_tab_bar_view.dart';
import '../models/rvm/subject_type_vm.dart';
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
  final DateTime timestamp;

  const SubjectPage({
    super.key,
    required this.subjectId,
    required this.timestamp,
  });

  @override
  State<SubjectPage> createState() => _SubjectPageState();
}

class _SubjectPageState extends StateX<SubjectPage> {
  late final _subjectBloc = use<SubjectBloc>();

  @override
  void initState() {
    super.initState();
    // @@TODO: Refresh when account changes.
    _subjectBloc.dispatch(GetSubject(subjectId: widget.subjectId));
  }

  @override
  void didUpdateWidget(covariant SubjectPage oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.timestamp != oldWidget.timestamp) {
      _subjectBloc.dispatch(GetSubject(subjectId: widget.subjectId));
    }
  }

  List<Widget> _buildTagChips(SubjectVm subject) {
    return subject.tags
        .map(
          (tag) => Padding(
            padding: const EdgeInsets.only(right: 8),
            child: Chip(
              label: Text(tag.name),
              labelStyle: GoogleFonts.righteous(
                color: const Color(0xffF8F9FA),
              ),
              backgroundColor: const Color(0xFF242423),
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
                color: const Color(0xFF242423),
              ),
              const SizedBox(width: 32),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      subject.name,
                      style: GoogleFonts.philosopher(fontSize: 31),
                    ),
                    const SizedBox(height: 12),
                    Row(children: _buildTagChips(subject)),
                  ],
                ),
              ),
              const SizedBox(width: 16),
              Column(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  Text(
                    'Submitted on',
                    style: GoogleFonts.raleway(),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    subject.submittedAtFormatted,
                    style: GoogleFonts.raleway(
                      fontSize: 16,
                    ),
                  ),
                  const SizedBox(height: 8),
                  RichText(
                    text: TextSpan(
                      children: [
                        TextSpan(
                          text: 'by ',
                          style: GoogleFonts.raleway(),
                        ),
                        TextSpan(
                          text: subject.submitterWalletAddressShort,
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
    var tabs = const [Text('Details'), Text('Promises')];

    return SizedBox(
      width: double.infinity,
      height: 800,
      child: Stack(
        children: [
          ContainedTabBarView(
            tabs: tabs,
            tabBarProperties: TabBarProperties(
              margin: const EdgeInsets.only(bottom: 8),
              width: 600,
              height: 40,
              indicator: ContainerTabIndicator(
                radius: BorderRadius.circular(8),
                color: Colors.indigo,
              ),
              labelColor: Colors.white,
              unselectedLabelColor: Colors.grey,
            ),
            tabBarViewProperties: const TabBarViewProperties(
              physics: NeverScrollableScrollPhysics(),
            ),
            views: _buildTabContents(subject),
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
  Widget buildX(BuildContext context) {
    return StreamBuilder(
      stream: _subjectBloc.subject$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return const SliverFillRemaining(
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
