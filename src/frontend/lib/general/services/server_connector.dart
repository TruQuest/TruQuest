import 'package:dio/dio.dart';

class ServerConnector {
  late final Dio dio;

  ServerConnector() {
    dio = Dio(
      BaseOptions(
        baseUrl: "http://localhost:5223",
      ),
    );
  }
}
