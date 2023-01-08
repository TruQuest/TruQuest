import 'new_thing_im.dart';

class CreateNewThingDraftCommand {
  final NewThingIm input;

  CreateNewThingDraftCommand({required this.input});

  Map<String, dynamic> toJson() {
    var map = <String, dynamic>{};

    map['input'] = input.toJson();

    return map;
  }
}
