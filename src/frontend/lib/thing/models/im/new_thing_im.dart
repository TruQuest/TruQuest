import 'evidence_im.dart';
import 'tag_im.dart';

class NewThingIm {
  final String subjectId;
  final String title;
  final String details;
  final String? imageUrl;
  final List<EvidenceIm> evidence;
  final List<TagIm> tags;

  NewThingIm({
    required this.subjectId,
    required this.title,
    required this.details,
    required this.imageUrl,
    required this.evidence,
    required this.tags,
  });

  Map<String, dynamic> toJson() {
    var map = <String, dynamic>{};

    map['subjectId'] = subjectId;
    map['title'] = title;
    map['details'] = details;
    map['imageUrl'] = imageUrl;
    map['evidence'] = evidence.map((e) => e.toJson()).toList();
    map['tags'] = tags.map((t) => t.toJson()).toList();

    return map;
  }
}
