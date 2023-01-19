import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flame/components.dart';
import 'package:flame/game.dart';
import 'package:flame/input.dart';

import 'ai_paddle.dart';
import 'ball.dart';
import 'fieldline.dart';
import 'player_paddle.dart';
import 'scoretext.dart';

class PongGame extends FlameGame
    with HasTappables, HasCollisionDetection, HasKeyboardHandlerComponents {
  PongGame();

  late final ScoreText aiPlayer;
  late final ScoreText player;

  @override
  Future<void> onLoad() async {
    addAll(
      [
        ScreenHitbox(),
        FieldLine(),
        aiPlayer = ScoreText.aiScore(),
        player = ScoreText.playerScore(),
        PlayerPaddle(),
        AIPaddle(),
        Ball(),
      ],
    );
  }

  @override
  @mustCallSuper
  KeyEventResult onKeyEvent(
    RawKeyEvent event,
    Set<LogicalKeyboardKey> keysPressed,
  ) {
    super.onKeyEvent(event, keysPressed);

    return KeyEventResult.handled;
  }

  @override
  void onDetach() {
    super.onDetach();
    print('onDetach');
  }

  @override
  void onRemove() {
    super.onRemove();
    print('onRemove');
  }
}
