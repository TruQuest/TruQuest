import 'package:flutter/widgets.dart';
import 'package:kiwi/kiwi.dart';

T _resolveDependency<T>() => KiwiContainer().resolve<T>();

abstract class IDisposable {
  void dispose();
}

class UseScope extends InheritedWidget {
  Map<Type, Object> _typeToInstance = {};
  final bool preserveOnRebuild;

  UseScope({
    super.key,
    required super.child,
    List<Object> useInstances = const [],
    this.preserveOnRebuild = true,
  }) {
    for (var instance in useInstances) {
      _typeToInstance[instance.runtimeType] = instance;
    }
  }

  @override
  bool updateShouldNotify(covariant UseScope oldWidget) {
    if (preserveOnRebuild) {
      _typeToInstance = oldWidget._typeToInstance;
    } else {
      for (var instance in oldWidget._typeToInstance.values) {
        if (instance is IDisposable) {
          instance.dispose();
        }
      }
    }

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
