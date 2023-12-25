enum WhitelistEntryTypeIm {
  email,
  signerAddress,
}

extension WhitelistEntryTypeImExtension on WhitelistEntryTypeIm {
  static WhitelistEntryTypeIm fromString(String value) {
    if (value == 'email') {
      return WhitelistEntryTypeIm.email;
    } else if (value == 'signer_address') {
      return WhitelistEntryTypeIm.signerAddress;
    }

    throw UnimplementedError();
  }
}
