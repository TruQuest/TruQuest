import 'package:flutter/material.dart';
import 'package:data_table_2/data_table_2.dart';

import '../extensions/iterable_extension.dart';
import '../models/rvm/verifier_lottery_participant_entry_vm.dart';

class LotteryParticipantsTable extends StatefulWidget {
  final List<VerifierLotteryParticipantEntryVm> entries;
  final String? currentUserId;

  const LotteryParticipantsTable({
    super.key,
    required this.entries,
    required this.currentUserId,
  });

  @override
  State<LotteryParticipantsTable> createState() =>
      _LotteryParticipantsTableState();
}

class _LotteryParticipantsTableState extends State<LotteryParticipantsTable> {
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
          DataColumn(label: Text('Block')),
          DataColumn(label: Text('User')),
          DataColumn(label: Text('Commitment')),
          DataColumn(label: Text('Nonce')),
        ],
        rows: widget.entries
            .mapIndexed(
              (e, i) => DataRow(
                color: MaterialStateProperty.all<Color?>(
                  e.userId == widget.currentUserId
                      ? Colors.black.withOpacity(0.2)
                      : null,
                ),
                cells: [
                  DataCell(Text((i + 1).toString())),
                  DataCell(Text(e.joinedBlockNumber?.toString() ?? '–')),
                  DataCell(Text(e.userId)),
                  DataCell(Text(e.dataHash)),
                  DataCell(Text(e.nonce?.toString() ?? '–')),
                ],
              ),
            )
            .toList(),
      ),
    );
  }
}
