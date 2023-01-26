class SupportingEvidenceVm {
  final String originUrl;
  final String ipfsCid;
  final String previewImageIpfsCid;

  SupportingEvidenceVm.fromMap(Map<String, dynamic> map)
      : originUrl = map['originUrl'],
        ipfsCid = map['ipfsCid'],
        previewImageIpfsCid = map['previewImageIpfsCid'];
}
