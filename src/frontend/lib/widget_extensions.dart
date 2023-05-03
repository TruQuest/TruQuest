import 'package:flutter/widgets.dart';
import 'package:kiwi/kiwi.dart';

T _resolveDependency<T>() => KiwiContainer().resolve<T>();

abstract class IDisposable {
  void dispose();
}

class ScopeX extends InheritedWidget {
  Map<Type, Object> _typeToInstance = {};
  final bool updatesShouldNotify;

  ScopeX({
    super.key,
    required super.child,
    List<Object> useInstances = const [],
    this.updatesShouldNotify = false,
  }) {
    for (var instance in useInstances) {
      _typeToInstance[instance.runtimeType] = instance;
    }
  }

  @override
  bool updateShouldNotify(covariant ScopeX oldWidget) {
    if (updatesShouldNotify) {
      for (var instance in oldWidget._typeToInstance.values) {
        if (instance is IDisposable) {
          instance.dispose();
        }
      }
    } else {
      _typeToInstance = oldWidget._typeToInstance;
    }

    return updatesShouldNotify;
  }

  T _resolve<T>() {
    Type t = T;
    if (!_typeToInstance.containsKey(t)) {
      _typeToInstance[t] = _resolveDependency<T>() as Object;
    }
    return _typeToInstance[t] as T;
  }
}

// ignore: must_be_immutable
abstract class StatelessWidgetX extends StatelessWidget {
  late BuildContext _context;

  StatelessWidgetX({super.key});

  T _resolveScoped<T>() {
    var provider = _context.dependOnInheritedWidgetOfExactType<ScopeX>();
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
    var provider = context.dependOnInheritedWidgetOfExactType<ScopeX>();
    return provider!._resolve<T>();
  }

  T use<T>() => _resolveDependency<T>();

  T useScoped<T>() => _resolveScoped<T>();
}
