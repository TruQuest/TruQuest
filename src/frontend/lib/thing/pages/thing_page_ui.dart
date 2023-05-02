// import 'dart:convert';

// import 'package:flutter/material.dart';
// import 'package:easy_sidemenu/easy_sidemenu.dart';
// import 'package:tab_container/tab_container.dart';

// import '../widgets/timeline_block.dart';
// import '../models/rvm/thing_state_vm.dart';
// import '../models/rvm/thing_vm.dart';
// import '../../general/contexts/document_view_context.dart';
// import '../../general/widgets/document_view.dart';
// import '../../general/widgets/evidence_view_block.dart';
// import '../../widget_extensions.dart';

// class ThingPageUi extends StatelessWidget {
//   final PageController _pageController = PageController();

//   ThingPageUi({super.key});

//   List<Widget> _buildTagChips(TextTheme textTheme) {
//     return ['Politics', 'Sport', 'IT']
//         .map((tag) => Padding(
//               padding: const EdgeInsets.only(right: 8),
//               child: Chip(
//                 label: Text(tag),
//                 labelStyle: textTheme.caption,
//                 backgroundColor: Colors.black12,
//               ),
//             ))
//         .toList();
//   }

//   Widget _buildHeader(BuildContext context) {
//     var textTheme = Theme.of(context).textTheme;

//     var thingInfo = Column(
//       crossAxisAlignment: CrossAxisAlignment.start,
//       children: [
//         Text(
//           'Thing title',
//           style: textTheme.titleLarge,
//         ),
//         SizedBox(height: 12),
//         Row(children: _buildTagChips(textTheme)),
//       ],
//     );

//     return Stack(
//       children: [
//         Padding(
//           padding: const EdgeInsets.only(bottom: 100),
//           child: ArcBannerImage('assets/images/olo.jpg'),
//         ),
//         Positioned(
//           bottom: 10,
//           left: 40,
//           right: 16,
//           child: Row(
//             crossAxisAlignment: CrossAxisAlignment.end,
//             mainAxisAlignment: MainAxisAlignment.end,
//             children: [
//               Poster(
//                 'assets/images/olo.jpg',
//                 height: 200,
//               ),
//               SizedBox(width: 16),
//               Expanded(child: thingInfo),
//             ],
//           ),
//         ),
//       ],
//     );
//   }

//   List<Widget> _buildTabs() {
//     return [
//       Icon(Icons.content_paste),
//       Icon(Icons.people),
//       Icon(Icons.poll_outlined),
//     ];
//   }

//   List<Widget> _buildTabsContents() {
//     var a = [
//       {"insert": "Flutter Quill"},
//       {
//         "attributes": {"header": 1},
//         "insert": "\n"
//       },
//       {"insert": "\nRich text editor for Flutter"},
//       {
//         "attributes": {"header": 2},
//         "insert": "\n"
//       },
//       {"insert": "Quill component for Flutter"},
//       {
//         "attributes": {"header": 3},
//         "insert": "\n"
//       },
//       {"insert": "This "},
//       {
//         "attributes": {"italic": true, "background": "transparent"},
//         "insert": "library"
//       },
//       {"insert": " supports "},
//       {
//         "attributes": {"bold": true, "background": "#ebd6ff"},
//         "insert": "mobile"
//       },
//       {"insert": " platform "},
//       {
//         "attributes": {"underline": true, "bold": true, "color": "#e60000"},
//         "insert": "only"
//       },
//       {
//         "attributes": {"color": "rgba(0, 0, 0, 0.847)"},
//         "insert": " and "
//       },
//       {
//         "attributes": {"strike": true, "color": "black"},
//         "insert": "web"
//       },
//       {"insert": " is not supported.\nYou are welcome to use "},
//       {
//         "attributes": {"link": "https://bulletjournal.us/home/index.html"},
//         "insert": "Bullet Journal"
//       },
//       {
//         "insert":
//             ":\nTrack personal and group journals (ToDo, Note, Ledger) from multiple views with timely reminders"
//       },
//       {
//         "attributes": {"list": "ordered"},
//         "insert": "\n"
//       },
//       {
//         "insert":
//             "Share your tasks and notes with teammates, and see changes as they happen in real-time, across all devices"
//       },
//       {
//         "attributes": {"list": "ordered"},
//         "insert": "\n"
//       },
//       {
//         "insert":
//             "Check out what you and your teammates are working on each day"
//       },
//       {
//         "attributes": {"list": "ordered"},
//         "insert": "\n"
//       },
//       {"insert": "\nSplitting bills with friends can never be easier."},
//       {
//         "attributes": {"list": "bullet"},
//         "insert": "\n"
//       },
//       {"insert": "Start creating a group and invite your friends to join."},
//       {
//         "attributes": {"list": "bullet"},
//         "insert": "\n"
//       },
//       {
//         "insert":
//             "Create a BuJo of Ledger type to see expense or balance summary."
//       },
//       {
//         "attributes": {"list": "bullet"},
//         "insert": "\n"
//       },
//       {
//         "insert":
//             "\nAttach one or multiple labels to tasks, notes or transactions. Later you can track them just using the label(s)."
//       },
//       {
//         "attributes": {"blockquote": true},
//         "insert": "\n"
//       },
//       {"insert": "\nvar BuJo = 'Bullet' + 'Journal'"},
//       {
//         "attributes": {"code-block": true},
//         "insert": "\n"
//       },
//       {"insert": "\nStart tracking in your browser"},
//       {
//         "attributes": {"indent": 1},
//         "insert": "\n"
//       },
//       {"insert": "Stop the timer on your phone"},
//       {
//         "attributes": {"indent": 1},
//         "insert": "\n"
//       },
//       {"insert": "All your time entries are synced"},
//       {
//         "attributes": {"indent": 2},
//         "insert": "\n"
//       },
//       {"insert": "between the phone apps"},
//       {
//         "attributes": {"indent": 2},
//         "insert": "\n"
//       },
//       {"insert": "and the website."},
//       {
//         "attributes": {"indent": 3},
//         "insert": "\n"
//       },
//       {"insert": "\n"},
//       {"insert": "\nCenter Align"},
//       {
//         "attributes": {"align": "center"},
//         "insert": "\n"
//       },
//       {"insert": "Right Align"},
//       {
//         "attributes": {"align": "right"},
//         "insert": "\n"
//       },
//       {"insert": "Justify Align"},
//       {
//         "attributes": {"align": "justify"},
//         "insert": "\n"
//       },
//       {"insert": "Have trouble finding things? "},
//       {
//         "attributes": {"list": "ordered"},
//         "insert": "\n"
//       },
//       {"insert": "Just type in the search bar"},
//       {
//         "attributes": {"indent": 1, "list": "ordered"},
//         "insert": "\n"
//       },
//       {"insert": "and easily find contents"},
//       {
//         "attributes": {"indent": 2, "list": "ordered"},
//         "insert": "\n"
//       },
//       {"insert": "across projects or folders."},
//       {
//         "attributes": {"indent": 2, "list": "ordered"},
//         "insert": "\n"
//       },
//       {"insert": "It matches text in your note or task."},
//       {
//         "attributes": {"indent": 1, "list": "ordered"},
//         "insert": "\n"
//       },
//       {"insert": "Enable reminders so that you will get notified by"},
//       {
//         "attributes": {"list": "ordered"},
//         "insert": "\n"
//       },
//       {"insert": "email"},
//       {
//         "attributes": {"indent": 1, "list": "ordered"},
//         "insert": "\n"
//       },
//       {"insert": "message on your phone"},
//       {
//         "attributes": {"indent": 1, "list": "ordered"},
//         "insert": "\n"
//       },
//       {"insert": "popup on the web site"},
//       {
//         "attributes": {"indent": 1, "list": "ordered"},
//         "insert": "\n"
//       },
//       {"insert": "Create a BuJo serving as project or folder"},
//       {
//         "attributes": {"list": "bullet"},
//         "insert": "\n"
//       },
//       {"insert": "Organize your"},
//       {
//         "attributes": {"indent": 1, "list": "bullet"},
//         "insert": "\n"
//       },
//       {"insert": "tasks"},
//       {
//         "attributes": {"indent": 2, "list": "bullet"},
//         "insert": "\n"
//       },
//       {"insert": "notes"},
//       {
//         "attributes": {"indent": 2, "list": "bullet"},
//         "insert": "\n"
//       },
//       {"insert": "transactions"},
//       {
//         "attributes": {"indent": 2, "list": "bullet"},
//         "insert": "\n"
//       },
//       {"insert": "under BuJo "},
//       {
//         "attributes": {"indent": 3, "list": "bullet"},
//         "insert": "\n"
//       },
//       {"insert": "See them in Calendar"},
//       {
//         "attributes": {"list": "bullet"},
//         "insert": "\n"
//       },
//       {"insert": "or hierarchical view"},
//       {
//         "attributes": {"indent": 1, "list": "bullet"},
//         "insert": "\n"
//       },
//       {"insert": "this is a check list"},
//       {
//         "attributes": {"list": "checked"},
//         "insert": "\n"
//       },
//       {"insert": "this is a uncheck list"},
//       {
//         "attributes": {"list": "unchecked"},
//         "insert": "\n"
//       },
//       {"insert": "Font "},
//       {
//         "attributes": {"font": "sans-serif"},
//         "insert": "Sans Serif"
//       },
//       {"insert": " "},
//       {
//         "attributes": {"font": "serif"},
//         "insert": "Serif"
//       },
//       {"insert": " "},
//       {
//         "attributes": {"font": "monospace"},
//         "insert": "Monospace"
//       },
//       {"insert": " Size "},
//       {
//         "attributes": {"size": "small"},
//         "insert": "Small"
//       },
//       {"insert": " "},
//       {
//         "attributes": {"size": "large"},
//         "insert": "Large"
//       },
//       {"insert": " "},
//       {
//         "attributes": {"size": "huge"},
//         "insert": "Huge"
//       },
//       {
//         "attributes": {"size": "15.0"},
//         "insert": "font size 15"
//       },
//       {"insert": " "},
//       {
//         "attributes": {"size": "35"},
//         "insert": "font size 35"
//       },
//       {"insert": " "},
//       {
//         "attributes": {"size": "20"},
//         "insert": "font size 20"
//       },
//       {
//         "attributes": {"token": "built_in"},
//         "insert": " diff"
//       },
//       {
//         "attributes": {"token": "operator"},
//         "insert": "-match"
//       },
//       {
//         "attributes": {"token": "literal"},
//         "insert": "-patch"
//       },
//       {"insert": "\n"}
//     ];

//     return [
//       ScopeX(
//         updatesShouldNotify: true,
//         useInstances: [
//           DocumentViewContext(
//             nameOrTitle: 'Thing title',
//             details: jsonEncode(a),
//             tags: [],
//             thing: ThingVm(
//               id: 'thingid',
//               state: ThingStateVm.draft,
//               title: 'Thing title',
//               details: 'Details',
//               imageIpfsCid: '',
//               croppedImageIpfsCid: '',
//               submitterId: 'asdasdasd',
//               subjectId: 'zxczxczc',
//               evidence: [],
//               tags: [],
//               fundedAwaitingConfirmation: null,
//             ),
//             signature: null,
//           ),
//         ],
//         child: DocumentView(
//           sideBlocks: [
//             TimelineBlock(),
//           ],
//           bottomBlock: EvidenceViewBlock(),
//         ),
//       ),
//       Center(
//         child: Text('Lottery'),
//       ),
//       Center(
//         child: Text('Poll'),
//       ),
//     ];
//   }

//   Widget _buildContent() {
//     return SizedBox(
//       width: double.infinity,
//       height: 800,
//       child: TabContainer(
//         controller: TabContainerController(length: 3),
//         tabEnd: 0.3,
//         colors: [
//           Colors.blue[200]!,
//           Colors.purple[200]!,
//           Colors.orange[200]!,
//         ],
//         isStringTabs: false,
//         tabEdge: TabEdge.top,
//         tabs: _buildTabs(),
//         children: _buildTabsContents(),
//       ),
//     );
//   }

//   Widget _buildPage(BuildContext context) {
//     return SingleChildScrollView(
//       child: Column(
//         children: [
//           _buildHeader(context),
//           SizedBox(height: 30),
//           _buildContent(),
//         ],
//       ),
//     );
//   }

//   @override
//   Widget build(BuildContext context) {
//     return Scaffold(
//       appBar: AppBar(),
//       body: Row(
//         children: [
//           SideMenu(
//             controller: _pageController,
//             style: SideMenuStyle(
//               displayMode: SideMenuDisplayMode.open,
//             ),
//             title: SizedBox(
//               width: double.infinity,
//               height: 200,
//               child: Center(child: Text('Smth')),
//             ),
//             items: [
//               SideMenuItem(
//                 priority: 1,
//                 icon: Icon(Icons.person_pin),
//                 title: 'Subjects',
//                 onTap: () => _pageController.jumpToPage(1),
//               ),
//               SideMenuItem(
//                 priority: 2,
//                 icon: Icon(Icons.note),
//                 title: 'Things',
//                 onTap: () => _pageController.jumpToPage(2),
//               ),
//               SideMenuItem(
//                 priority: 3,
//                 icon: Icon(Icons.question_answer),
//                 title: 'How To',
//                 onTap: () => _pageController.jumpToPage(3),
//               ),
//               SideMenuItem(
//                 priority: 4,
//                 icon: Icon(Icons.theater_comedy_outlined),
//                 title: 'Pong!',
//                 onTap: () => _pageController.jumpToPage(4),
//               ),
//               SideMenuItem(
//                 priority: 5,
//                 icon: Icon(Icons.route),
//                 title: 'Go To',
//                 onTap: () => _pageController.jumpToPage(5),
//               ),
//             ],
//           ),
//           Expanded(
//             child: PageView.builder(
//               controller: _pageController,
//               physics: NeverScrollableScrollPhysics(),
//               itemBuilder: (context, index) {
//                 return _buildPage(context);
//               },
//             ),
//           ),
//         ],
//       ),
//     );
//   }
// }

// class ArcBannerImage extends StatelessWidget {
//   final String imageUrl;

//   ArcBannerImage(this.imageUrl, {super.key});

//   @override
//   Widget build(BuildContext context) {
//     return ClipPath(
//       clipper: ArcClipper(),
//       child: AspectRatio(
//         aspectRatio: 16 / 9,
//         child: Image.asset(
//           imageUrl,
//           fit: BoxFit.cover,
//         ),
//       ),
//     );
//   }
// }

// class ArcClipper extends CustomClipper<Path> {
//   @override
//   Path getClip(Size size) {
//     var path = Path();
//     path.lineTo(0.0, size.height - 100);

//     var firstControlPoint = Offset(size.width / 4, size.height);
//     var firstPoint = Offset(size.width / 2, size.height);
//     path.quadraticBezierTo(firstControlPoint.dx, firstControlPoint.dy,
//         firstPoint.dx, firstPoint.dy);

//     var secondControlPoint = Offset(size.width - (size.width / 4), size.height);
//     var secondPoint = Offset(size.width, size.height - 100);
//     path.quadraticBezierTo(secondControlPoint.dx, secondControlPoint.dy,
//         secondPoint.dx, secondPoint.dy);

//     path.lineTo(size.width, 0.0);
//     path.close();

//     return path;
//   }

//   @override
//   bool shouldReclip(CustomClipper<Path> oldClipper) => false;
// }

// class Poster extends StatelessWidget {
//   static const posterAspectRatio = 16 / 9;

//   final String posterUrl;
//   final double height;

//   Poster(
//     this.posterUrl, {
//     super.key,
//     this.height = 200,
//   });

//   @override
//   Widget build(BuildContext context) {
//     var width = posterAspectRatio * height;

//     return Material(
//       borderRadius: BorderRadius.circular(12),
//       elevation: 10,
//       clipBehavior: Clip.antiAlias,
//       child: Image.asset(
//         posterUrl,
//         fit: BoxFit.cover,
//         width: width,
//         height: height,
//       ),
//     );
//   }
// }
