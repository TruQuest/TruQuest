import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';

// ignore: must_be_immutable
class RestrictWhenUnauthorizedButton extends StatelessWidgetX {
  late final _userBloc = use<UserBloc>();

  final Widget child;

  RestrictWhenUnauthorizedButton({super.key, required this.child});

  @override
  Widget buildX(BuildContext context) {
    return Stack(
      children: [
        child,
        Positioned.fill(
          child: StreamBuilder(
            stream: _userBloc.currentUser$,
            builder: (context, snapshot) {
              var userId = snapshot.data?.id;
              return IgnorePointer(
                ignoring: userId != null,
                child: GestureDetector(
                  onTap: () => showDialog(
                    context: context,
                    builder: (_) => AlertDialog(
                      backgroundColor: const Color(0xFF242423),
                      title: Text(
                        'Read-only mode',
                        style: GoogleFonts.philosopher(
                          color: Colors.white,
                        ),
                      ),
                      content: Text(
                        'Please sign-in to get access',
                        style: GoogleFonts.raleway(
                          color: Colors.white,
                        ),
                      ),
                    ),
                  ),
                ),
              );
            },
          ),
        ),
      ],
    );
  }
}
