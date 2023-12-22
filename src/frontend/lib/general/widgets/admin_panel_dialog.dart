import 'package:flutter/material.dart';
import 'package:contained_tab_bar_view/contained_tab_bar_view.dart'
    show TabBarProperties, TabBarViewProperties, ContainerTabIndicator;
import 'package:multi_select_flutter/multi_select_flutter.dart';

import '../../widget_extensions.dart';
import '../models/vm/get_contracts_states_rvm.dart';
import 'contained_tab_bar_view.dart';

// ignore: must_be_immutable
class AdminPanelDialog extends StatelessWidgetX {
  final GetContractsStatesRvm vm;

  AdminPanelDialog({super.key, required this.vm});

  Widget _buildGeneralInfoTab() {
    var info = vm.truQuestInfo;
    return Container(
      width: double.infinity,
      height: double.infinity,
      decoration: BoxDecoration(
        color: Color(0xFF413C69),
        borderRadius: BorderRadius.circular(16),
      ),
      padding: EdgeInsets.fromLTRB(20, 10, 20, 10),
      child: SingleChildScrollView(
        child: Column(
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Version: ${info.version}',
                  style: TextStyle(color: Colors.white),
                ),
                ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.black,
                    foregroundColor: Colors.white,
                  ),
                  child: Text('Stop the world: ${info.stopTheWorld}'),
                  onPressed: () {},
                ),
                ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.black,
                    foregroundColor: Colors.white,
                  ),
                  child: Text('Withdrawals enabled: ${info.withdrawalsEnabled}'),
                  onPressed: () {},
                ),
              ],
            ),
            const SizedBox(height: 20),
            Table(
              columnWidths: const {
                0: FixedColumnWidth(300),
                1: FixedColumnWidth(200),
              },
              border: TableBorder.symmetric(
                inside: BorderSide(color: Colors.white),
              ),
              children: [
                TableRow(
                  children: const [
                    Text('Contract name'),
                    Text('Contract address'),
                  ],
                ),
                TableRow(
                  children: [
                    Text('Truthserum'),
                    Text(info.truthserumAddress),
                  ],
                ),
                TableRow(
                  children: [
                    Text('RestrictedAccess'),
                    Text(info.restrictedAccessAddress),
                  ],
                ),
                TableRow(
                  children: [
                    Text('TruQuest'),
                    Text(info.truQuestAddress),
                  ],
                ),
                TableRow(
                  children: [
                    Text('ThingValidationVerifierLottery'),
                    Text(info.thingValidationVerifierLotteryAddress),
                  ],
                ),
                TableRow(
                  children: [
                    Text('ThingValidationPoll'),
                    Text(info.thingValidationPollAddress),
                  ],
                ),
                TableRow(
                  children: [
                    Text('SettlementProposalAssessmentVerifierLottery'),
                    Text(info.settlementProposalAssessmentVerifierLotteryAddress),
                  ],
                ),
                TableRow(
                  children: [
                    Text('SettlementProposalAssessmentPoll'),
                    Text(info.settlementProposalAssessmentPollAddress),
                  ],
                ),
              ],
            ),
            const SizedBox(height: 20),
            Table(
              defaultColumnWidth: const FixedColumnWidth(200),
              border: TableBorder.all(color: Colors.white),
              children: [
                TableRow(
                  children: const [
                    Text('Field name'),
                    Text('Field value'),
                  ],
                ),
                TableRow(
                  children: [
                    Text('s_treasury'),
                    Text(info.treasury.toString()),
                  ],
                ),
                TableRow(
                  children: [
                    Text('s_thingStake'),
                    Text(info.thingStake.toString()),
                  ],
                ),
                TableRow(
                  children: [
                    Text('s_verifierStake'),
                    Text(info.verifierStake.toString()),
                  ],
                ),
                TableRow(
                  children: [
                    Text('s_settlementProposalStake'),
                    Text(info.settlementProposalStake.toString()),
                  ],
                ),
                TableRow(
                  children: [
                    Text('s_thingAcceptedReward'),
                    Text(info.thingAcceptedReward.toString()),
                  ],
                ),
                TableRow(
                  children: [
                    Text('s_thingRejectedPenalty'),
                    Text(info.thingRejectedPenalty.toString()),
                  ],
                ),
                TableRow(
                  children: [
                    Text('s_verifierReward'),
                    Text(info.verifierReward.toString()),
                  ],
                ),
                TableRow(
                  children: [
                    Text('s_verifierPenalty'),
                    Text(info.verifierPenalty.toString()),
                  ],
                ),
                TableRow(
                  children: [
                    Text('s_settlementProposalAcceptedReward'),
                    Text(info.settlementProposalAcceptedReward.toString()),
                  ],
                ),
                TableRow(
                  children: [
                    Text('s_settlementProposalRejectedPenalty'),
                    Text(info.settlementProposalRejectedPenalty.toString()),
                  ],
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildUsersTab() {
    return Container(
      width: double.infinity,
      height: double.infinity,
      decoration: BoxDecoration(
        color: Color.fromARGB(255, 67, 75, 145),
        borderRadius: BorderRadius.circular(16),
      ),
      padding: EdgeInsets.fromLTRB(20, 10, 20, 10),
      child: SingleChildScrollView(
        child: Column(
          children: [
            Container(
              decoration: const BoxDecoration(
                border: Border.symmetric(
                  horizontal: BorderSide(
                    color: Colors.white,
                    width: 2,
                  ),
                ),
              ),
              padding: const EdgeInsets.only(bottom: 4),
              child: MultiSelectDialogField<String>(
                decoration: const BoxDecoration(
                  border: Border(
                    bottom: BorderSide(
                      color: Colors.transparent,
                      width: 2,
                    ),
                  ),
                ),
                title: const Text('Whitelisted addresses'),
                buttonText: const Text(
                  'Addresses',
                  style: TextStyle(color: Colors.white),
                ),
                searchable: true,
                dialogWidth: 400,
                items: vm.whitelistedWalletAddresses
                    .map((address) => MultiSelectItem<String>(address, address.substring(0, 10)))
                    .toList(),
                listType: MultiSelectListType.LIST,
                onConfirm: (addresses) {},
              ),
            ),
            const SizedBox(height: 8),
            ElevatedButton(
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.black,
                foregroundColor: Colors.white,
              ),
              child: Text('Remove access from selected addresses'),
              onPressed: () {},
            ),
            const SizedBox(height: 20),
            Container(
              width: 500,
              decoration: BoxDecoration(
                border: Border.all(color: Colors.white),
              ),
              padding: const EdgeInsets.fromLTRB(12, 8, 12, 8),
              child: Column(
                children: [
                  Row(
                    children: [
                      Expanded(
                        child: TextField(
                          decoration: InputDecoration(
                            hintText: 'Email to whitelist',
                          ),
                        ),
                      ),
                      IconButton(
                        icon: Icon(Icons.add_box_outlined),
                        onPressed: () {},
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      Expanded(
                        child: TextField(
                          decoration: InputDecoration(
                            hintText: 'Find wallet address by email',
                          ),
                        ),
                      ),
                      IconButton(
                        icon: Icon(Icons.add_box_outlined),
                        onPressed: () {},
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      Expanded(
                        child: TextField(
                          decoration: InputDecoration(
                            hintText: 'Give access to wallet address',
                          ),
                        ),
                      ),
                      IconButton(
                        icon: Icon(Icons.add_box_outlined),
                        onPressed: () {},
                      ),
                    ],
                  ),
                ],
              ),
            ),
            const SizedBox(height: 20),
            Table(
              defaultColumnWidth: const FixedColumnWidth(200),
              border: TableBorder.all(color: Colors.white),
              children: [
                TableRow(
                  children: const [
                    Text('Id'),
                    Text('Wallet address'),
                    Text('Balance'),
                    Text('Staked balance'),
                  ],
                ),
                ...vm.users.map(
                  (user) => TableRow(
                    children: [
                      Text(user.id),
                      Text(user.walletAddress),
                      Text(user.balance.toString()),
                      Text(user.stakedBalance.toString()),
                    ],
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget buildX(BuildContext context) {
    // @@NOTE: Use AlertDialog because SimpleDialog is automatically scrollable, which I don't want.
    return AlertDialog(
      backgroundColor: const Color(0xFF242423),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
      ),
      contentPadding: const EdgeInsets.only(top: 12),
      clipBehavior: Clip.antiAlias,
      content: SizedBox(
        width: 1024,
        height: 768,
        child: ContainedTabBarView(
          tabs: const [
            Text('General info'),
            Text('Users'),
            // Text('Subjects/Things/Proposals'),
          ],
          tabBarProperties: TabBarProperties(
            margin: const EdgeInsets.only(bottom: 8),
            width: 600,
            height: 40,
            indicator: ContainerTabIndicator(
              radius: BorderRadius.circular(8),
              color: Colors.indigo,
            ),
            labelColor: Colors.white,
            unselectedLabelColor: Colors.grey,
          ),
          tabBarViewProperties: const TabBarViewProperties(
            physics: NeverScrollableScrollPhysics(),
          ),
          views: [
            _buildGeneralInfoTab(),
            _buildUsersTab(),
          ],
        ),
      ),
    );
  }
}
