import 'package:flutter/material.dart';
import 'package:data_table_2/data_table_2.dart';

import '../extensions/iterable_extension.dart';
import '../models/rvm/verifier_vm.dart';

class VerifiersTable extends StatelessWidget {
  final List<VerifierVm> verifiers;

  const VerifiersTable({super.key, required this.verifiers});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(16),
      child: DataTable2(
        // columnSpacing: 12,
        // horizontalMargin: 12,
        // minWidth: 600,
        columns: [
          DataColumn(label: Text('#'), numeric: true),
          DataColumn(label: Text('User Id')),
          DataColumn(label: Text('Username')),
          DataColumn(label: Text('Voted off-chain at')),
          DataColumn(label: Text('Voted on-chain at #')),
        ],
        rows: verifiers
            .mapIndexed(
              (v, i) => DataRow(
                cells: [
                  DataCell(Text((i + 1).toString())),
                  DataCell(Text(v.verifierId)),
                  DataCell(Text(v.username)),
                  DataCell(Text('–')),
                  DataCell(Text('–')),
                ],
              ),
            )
            .toList(),
      ),
    );
  }
}
