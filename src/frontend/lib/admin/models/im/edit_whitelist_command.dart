import 'whitelist_entry_type_im.dart';

class EditWhitelistCommand {
  final WhitelistEntryTypeIm entryType;
  final String entry;

  EditWhitelistCommand({
    required this.entryType,
    required this.entry,
  });

  Map<String, dynamic> toJson() => {
        'entryType': entryType.index,
        'entry': entry,
      };
}
