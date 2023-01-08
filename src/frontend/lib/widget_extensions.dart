// ignore_for_file: must_be_immutable

import "package:flutter/widgets.dart";
import "package:kiwi/kiwi.dart";

T _resolveDependency<T>() => KiwiContainer().resolve<T>();

class UseScope extends InheritedWidget {
  Map<Type, Object> _typeToInstance = {};

  UseScope({super.key, required super.child});

  @override
  bool updateShouldNotify(covariant UseScope oldWidget) {
    _typeToInstance = oldWidget._typeToInstance;
    return false;
  }

  T _resolve<T>() {
    Type t = T;
    if (!_typeToInstance.containsKey(t)) {
      _typeToInstance[t] = _resolveDependency<T>() as Object;
    }
    return _typeToInstance[t] as T;
  }
}

abstract class StatelessWidgetX extends StatelessWidget {
  late BuildContext _context;

  StatelessWidgetX({super.key});

  T _resolveScoped<T>() {
    var provider = _context.dependOnInheritedWidgetOfExactType<UseScope>();
    return provider!._resolve<T>();
  }

  T use<T>() => _resolveDependency<T>();

  T useScoped<T>() => _resolveScoped<T>();

  @override
  Widget build(BuildContext context) {
    _context = context;
    return buildX(context);
  }

  Widget buildX(BuildContext context);
}

abstract class StateX<TWidget extends StatefulWidget> extends State<TWidget> {
  T _resolveScoped<T>() {
    var provider = context.dependOnInheritedWidgetOfExactType<UseScope>();
    return provider!._resolve<T>();
  }

  T use<T>() => _resolveDependency<T>();

  T useScoped<T>() => _resolveScoped<T>();
}
