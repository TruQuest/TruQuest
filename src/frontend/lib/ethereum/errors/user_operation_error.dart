class UserOperationError {
  late final int? code;
  late final String message;
  late final bool isFurtherDecodable;

  @override
  String toString() => message;

  UserOperationError({this.message = 'Something went wrong'})
      : code = null,
        isFurtherDecodable = false;

  UserOperationError.fromMap(Map<String, dynamic> map) {
    code = map['code'];
    var message = map['message'] as String;
    if (code == -32521) {
      int index = message.indexOf('0x');
      if (index >= 0) {
        this.message = message.substring(index);
        isFurtherDecodable = true;
        return;
      }
    }

    this.message = message;
    isFurtherDecodable = false;
  }
}
