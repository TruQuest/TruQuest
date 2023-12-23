import 'dart:async';

import 'package:flutter/material.dart';
import 'package:rxdart/rxdart.dart';

class SearchableList<T> extends StatefulWidget {
  final List<T> values;
  final List<T> Function(List<T> allValues, String searchTerm) onSearch;
  final Widget Function(T value) onDisplay;
  final double? width;
  final double height;
  final Color color;
  final BorderRadius borderRadius;

  const SearchableList({
    super.key,
    required this.values,
    required this.onSearch,
    required this.onDisplay,
    this.width,
    required this.height,
    required this.color,
    required this.borderRadius,
  });

  @override
  State<SearchableList<T>> createState() => SsearchableListState<T>();
}

class SsearchableListState<T> extends State<SearchableList<T>> {
  final _inputChannel = BehaviorSubject<String>();
  late final StreamSubscription<String> _input$$;

  late List<T> _valuesToDisplay;

  @override
  void initState() {
    super.initState();
    _valuesToDisplay = widget.values;
    _input$$ = _inputChannel.stream.debounceTime(const Duration(seconds: 1)).listen((searchTerm) {
      var valuesToDisplay = searchTerm.isNotEmpty ? widget.onSearch(widget.values, searchTerm) : widget.values;
      setState(() => _valuesToDisplay = valuesToDisplay);
    });
  }

  @override
  void dispose() {
    _input$$.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      width: widget.width,
      height: widget.height,
      decoration: BoxDecoration(
        color: widget.color,
        borderRadius: widget.borderRadius,
      ),
      padding: const EdgeInsets.fromLTRB(8, 10, 8, 10),
      child: Column(
        children: [
          TextField(
            onChanged: _inputChannel.add,
            decoration: InputDecoration(
              hintText: 'Search',
            ),
          ),
          const SizedBox(height: 8),
          Expanded(
            child: ListView.separated(
              itemBuilder: (context, index) {
                var value = _valuesToDisplay[index];
                return widget.onDisplay(value);
              },
              separatorBuilder: (_, __) => const Divider(),
              itemCount: _valuesToDisplay.length,
            ),
          ),
        ],
      ),
    );
  }
}
