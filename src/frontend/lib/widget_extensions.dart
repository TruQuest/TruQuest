// ignore_for_file: must_be_immutable

import "package:flutter/widgets.dart";
import "package:kiwi/kiwi.dart";

T _resolveDependency<T>() => KiwiContainer().resolve<T>();

class ServiceScope extends InheritedWidget {
  Map<Type, Object> _typeToInstance = {};

  ServiceScope({super.key, required super.child});

  @override
  bool updateShouldNotify(covariant ServiceScope oldWidget) {
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

abstract class _BaseStatelessWidgetInject extends StatelessWidget {
  late BuildContext _context;

  _BaseStatelessWidgetInject({super.key});

  @override
  Widget build(BuildContext context) {
    _context = context;
    return buildInject(context);
  }

  Widget buildInject(BuildContext context);

  T _resolveScoped<T>() {
    var provider = _context.dependOnInheritedWidgetOfExactType<ServiceScope>();
    return provider!._resolve<T>();
  }
}

abstract class StatelessWidgetInject<TDependency>
    extends _BaseStatelessWidgetInject {
  late final TDependency service = _resolveDependency<TDependency>();
  late final TDependency scopedService = _resolveScoped<TDependency>();

  StatelessWidgetInject({super.key});
}

abstract class StatelessWidgetInject2<TDependency1, TDependency2>
    extends _BaseStatelessWidgetInject {
  late final TDependency1 service1 = _resolveDependency<TDependency1>();
  late final TDependency1 scopedService1 = _resolveScoped<TDependency1>();

  late final TDependency2 service2 = _resolveDependency<TDependency2>();
  late final TDependency2 scopedService2 = _resolveScoped<TDependency2>();

  StatelessWidgetInject2({super.key});
}

abstract class StatelessWidgetInject3<TDependency1, TDependency2, TDependency3>
    extends _BaseStatelessWidgetInject {
  late final TDependency1 service1 = _resolveDependency<TDependency1>();
  late final TDependency1 scopedService1 = _resolveScoped<TDependency1>();

  late final TDependency2 service2 = _resolveDependency<TDependency2>();
  late final TDependency2 scopedService2 = _resolveScoped<TDependency2>();

  late final TDependency3 service3 = _resolveDependency<TDependency3>();
  late final TDependency3 scopedService3 = _resolveScoped<TDependency3>();

  StatelessWidgetInject3({super.key});
}

abstract class StatelessWidgetInject4<TDependency1, TDependency2, TDependency3,
    TDependency4> extends _BaseStatelessWidgetInject {
  late final TDependency1 service1 = _resolveDependency<TDependency1>();
  late final TDependency1 scopedService1 = _resolveScoped<TDependency1>();

  late final TDependency2 service2 = _resolveDependency<TDependency2>();
  late final TDependency2 scopedService2 = _resolveScoped<TDependency2>();

  late final TDependency3 service3 = _resolveDependency<TDependency3>();
  late final TDependency3 scopedService3 = _resolveScoped<TDependency3>();

  late final TDependency4 service4 = _resolveDependency<TDependency4>();
  late final TDependency4 scopedService4 = _resolveScoped<TDependency4>();

  StatelessWidgetInject4({super.key});
}

abstract class _BaseStateInject<TWidget extends StatefulWidget>
    extends State<TWidget> {
  T _resolveScoped<T>() {
    var provider = context.dependOnInheritedWidgetOfExactType<ServiceScope>();
    return provider!._resolve<T>();
  }
}

abstract class StateInject<TWidget extends StatefulWidget, TDependency>
    extends _BaseStateInject<TWidget> {
  late final TDependency service = _resolveDependency<TDependency>();
  late final TDependency scopedService = _resolveScoped<TDependency>();
}

abstract class StateInject2<TWidget extends StatefulWidget, TDependency1,
    TDependency2> extends _BaseStateInject<TWidget> {
  late final TDependency1 service1 = _resolveDependency<TDependency1>();
  late final TDependency1 scopedService1 = _resolveScoped<TDependency1>();

  late final TDependency2 service2 = _resolveDependency<TDependency2>();
  late final TDependency2 scopedService2 = _resolveScoped<TDependency2>();
}

abstract class StateInject3<TWidget extends StatefulWidget, TDependency1,
    TDependency2, TDependency3> extends _BaseStateInject<TWidget> {
  late final TDependency1 service1 = _resolveDependency<TDependency1>();
  late final TDependency1 scopedService1 = _resolveScoped<TDependency1>();

  late final TDependency2 service2 = _resolveDependency<TDependency2>();
  late final TDependency2 scopedService2 = _resolveScoped<TDependency2>();

  late final TDependency3 service3 = _resolveDependency<TDependency3>();
  late final TDependency3 scopedService3 = _resolveScoped<TDependency3>();
}

abstract class StateInject4<
    TWidget extends StatefulWidget,
    TDependency1,
    TDependency2,
    TDependency3,
    TDependency4> extends _BaseStateInject<TWidget> {
  late final TDependency1 service1 = _resolveDependency<TDependency1>();
  late final TDependency1 scopedService1 = _resolveScoped<TDependency1>();

  late final TDependency2 service2 = _resolveDependency<TDependency2>();
  late final TDependency2 scopedService2 = _resolveScoped<TDependency2>();

  late final TDependency3 service3 = _resolveDependency<TDependency3>();
  late final TDependency3 scopedService3 = _resolveScoped<TDependency3>();

  late final TDependency4 service4 = _resolveDependency<TDependency4>();
  late final TDependency4 scopedService4 = _resolveScoped<TDependency4>();
}
