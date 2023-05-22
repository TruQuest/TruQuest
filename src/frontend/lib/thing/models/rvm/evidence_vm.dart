// @@??: Move to general?
class EvidenceVm {
  final String id;
  final String originUrl;
  final String ipfsCid;
  final String previewImageIpfsCid;

  EvidenceVm.fromMap(Map<String, dynamic> map)
      : id = map['id'],
        originUrl = map['originUrl'],
        ipfsCid = map['ipfsCid'],
        previewImageIpfsCid = map['previewImageIpfsCid'];
}
