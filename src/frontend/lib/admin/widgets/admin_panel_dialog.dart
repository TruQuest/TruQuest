import 'package:flutter/material.dart';
import 'package:contained_tab_bar_view/contained_tab_bar_view.dart'
    show TabBarProperties, TabBarViewProperties, ContainerTabIndicator;

import '../../general/widgets/contained_tab_bar_view.dart';
import '../../general/widgets/searchable_list.dart';
import '../../widget_extensions.dart';
import '../models/vm/get_contracts_states_rvm.dart';
import '../models/vm/poll_vm.dart';
import '../models/vm/settlement_proposal_assessment_verifier_lottery_vm.dart';
import '../models/vm/thing_validation_verifier_lottery_vm.dart';

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
                Text(
                  'Stop the world: ${info.stopTheWorld}',
                  style: TextStyle(color: Colors.white),
                ),
                Text(
                  'Withdrawals enabled: ${info.withdrawalsEnabled}',
                  style: TextStyle(color: Colors.white),
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
            SearchableList(
              values: vm.whitelistedWalletAddresses,
              onSearch: (allAddresses, searchTerm) {
                var regex = RegExp(searchTerm, caseSensitive: false);
                return allAddresses.where((a) => regex.hasMatch(a)).toList();
              },
              onDisplay: (address) => ListTile(title: Text(address)),
              width: 300,
              height: 500,
              color: Colors.white,
              borderRadius: BorderRadius.circular(6),
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

  Widget _buildSubjectsThingsProposalsTab() {
    return Container(
      width: double.infinity,
      height: double.infinity,
      decoration: BoxDecoration(
        color: Color.fromARGB(255, 67, 75, 145),
        borderRadius: BorderRadius.circular(16),
      ),
      padding: EdgeInsets.fromLTRB(20, 10, 20, 10),
      child: ListView.builder(
        itemBuilder: (context, index) {
          var subject = vm.subjects[index];
          return Container(
            margin: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              color: Colors.amber,
              borderRadius: BorderRadius.circular(12),
            ),
            clipBehavior: Clip.antiAlias,
            child: Column(
              children: [
                Row(
                  children: [
                    Text(subject.name),
                    const SizedBox(width: 12),
                    Text(subject.id),
                  ],
                ),
                const SizedBox(height: 10),
                SizedBox(
                  height: 400,
                  child: GridView.builder(
                    gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                      crossAxisCount: 3,
                      crossAxisSpacing: 16,
                      mainAxisExtent: 250,
                      mainAxisSpacing: 10,
                    ),
                    itemBuilder: (context, index) {
                      var thing = subject.things[index];
                      var proposal = thing.settlementProposal;

                      return Container(
                        decoration: BoxDecoration(
                          color: Colors.teal[100]!,
                          borderRadius: BorderRadius.circular(6),
                        ),
                        padding: const EdgeInsets.fromLTRB(10, 8, 10, 8),
                        child: Column(
                          children: [
                            Text(thing.title),
                            Text(thing.id),
                            Row(
                              children: [
                                Expanded(
                                  child: InkWell(
                                    onTap: thing.lottery != null
                                        ? () => showDialog(
                                              context: context,
                                              builder: (_) => _buildThingLotteryDialog(thing.lottery!),
                                            )
                                        : null,
                                    child: Container(
                                      height: 50,
                                      decoration: BoxDecoration(
                                        color: thing.lottery != null ? Colors.pink[100]! : Colors.transparent,
                                        border: Border.all(color: Colors.pink[100]!),
                                        borderRadius: BorderRadius.circular(4),
                                      ),
                                      alignment: Alignment.center,
                                      child: Text('Lottery'),
                                    ),
                                  ),
                                ),
                                const SizedBox(width: 10),
                                Expanded(
                                  child: InkWell(
                                    onTap: thing.poll != null
                                        ? () => showDialog(
                                              context: context,
                                              builder: (_) => _buildPollDialog('Thing validation poll', thing.poll!),
                                            )
                                        : null,
                                    child: Container(
                                      height: 50,
                                      decoration: BoxDecoration(
                                        color: thing.poll != null ? Colors.cyan[100]! : Colors.transparent,
                                        border: Border.all(color: Colors.cyan[100]!),
                                        borderRadius: BorderRadius.circular(4),
                                      ),
                                      alignment: Alignment.center,
                                      child: Text('Poll'),
                                    ),
                                  ),
                                ),
                              ],
                            ),
                            Container(
                              width: double.infinity,
                              height: 120,
                              decoration: BoxDecoration(
                                color: proposal != null ? Colors.deepPurple[300]! : Colors.transparent,
                                border: Border.all(color: Colors.deepPurple[300]!),
                                borderRadius: BorderRadius.circular(6),
                              ),
                              alignment: Alignment.center,
                              padding: const EdgeInsets.fromLTRB(6, 4, 6, 4),
                              child: proposal != null
                                  ? Column(
                                      children: [
                                        Text(proposal.title),
                                        Text(proposal.id),
                                        Row(
                                          children: [
                                            Expanded(
                                              child: InkWell(
                                                onTap: proposal.lottery != null
                                                    ? () => showDialog(
                                                          context: context,
                                                          builder: (_) => _buildProposalLotteryDialog(
                                                            proposal.lottery!,
                                                          ),
                                                        )
                                                    : null,
                                                child: Container(
                                                  height: 40,
                                                  decoration: BoxDecoration(
                                                    color: proposal.lottery != null
                                                        ? Colors.pink[100]!
                                                        : Colors.transparent,
                                                    border: Border.all(color: Colors.pink[100]!),
                                                    borderRadius: BorderRadius.circular(4),
                                                  ),
                                                  alignment: Alignment.center,
                                                  child: Text('Lottery'),
                                                ),
                                              ),
                                            ),
                                            const SizedBox(width: 6),
                                            Expanded(
                                              child: InkWell(
                                                onTap: proposal.poll != null
                                                    ? () => showDialog(
                                                          context: context,
                                                          builder: (_) => _buildPollDialog(
                                                            'Settlement proposal assessment poll',
                                                            proposal.poll!,
                                                          ),
                                                        )
                                                    : null,
                                                child: Container(
                                                  height: 40,
                                                  decoration: BoxDecoration(
                                                    color:
                                                        proposal.poll != null ? Colors.cyan[100]! : Colors.transparent,
                                                    border: Border.all(color: Colors.cyan[100]!),
                                                    borderRadius: BorderRadius.circular(4),
                                                  ),
                                                  alignment: Alignment.center,
                                                  child: Text('Poll'),
                                                ),
                                              ),
                                            ),
                                          ],
                                        ),
                                      ],
                                    )
                                  : Text('No settlement proposal'),
                            ),
                          ],
                        ),
                      );
                    },
                    itemCount: subject.things.length,
                  ),
                ),
              ],
            ),
          );
        },
        itemCount: vm.subjects.length,
      ),
    );
  }

  Widget _buildThingLotteryDialog(ThingValidationVerifierLotteryVm lottery) {
    return AlertDialog(
      title: Text('Thing validation verifier lottery'),
      content: Column(
        children: [
          Text('Block: ${lottery.orchestratorCommitment.l1BlockNumber}'),
          Text('DataHash: ${lottery.orchestratorCommitment.dataHash}'),
          Text('UserXorDataHash: ${lottery.orchestratorCommitment.userXorDataHash}'),
          SearchableList(
            values: lottery.participants,
            onSearch: (allParticipants, searchTerm) {
              var regex = RegExp(searchTerm, caseSensitive: false);
              return allParticipants.where((p) => regex.hasMatch(p.walletAddress)).toList();
            },
            onDisplay: (participant) => ListTile(
              leading: CircleAvatar(
                backgroundColor: Colors.green[200]!,
                radius: 15,
                child: Text(participant.l1BlockNumber.toString()),
              ),
              title: Text(participant.walletAddress),
              subtitle: Text(participant.userId),
            ),
            width: 300,
            height: 500,
            color: Colors.white,
            borderRadius: BorderRadius.circular(6),
          ),
        ],
      ),
    );
  }

  Widget _buildProposalLotteryDialog(SettlementProposalAssessmentVerifierLotteryVm lottery) {
    return AlertDialog(
      title: Text('Settlement proposal assessment verifier lottery'),
      content: SingleChildScrollView(
        child: Column(
          children: [
            Text('Block: ${lottery.orchestratorCommitment.l1BlockNumber}'),
            Text('DataHash: ${lottery.orchestratorCommitment.dataHash}'),
            Text('UserXorDataHash: ${lottery.orchestratorCommitment.userXorDataHash}'),
            SearchableList(
              values: lottery.claimants,
              onSearch: (allClaimants, searchTerm) {
                var regex = RegExp(searchTerm, caseSensitive: false);
                return allClaimants.where((c) => regex.hasMatch(c.walletAddress)).toList();
              },
              onDisplay: (claimant) => ListTile(
                leading: CircleAvatar(
                  backgroundColor: Colors.green[200]!,
                  radius: 15,
                  child: Text(claimant.l1BlockNumber.toString()),
                ),
                title: Text(claimant.walletAddress),
                subtitle: Text(claimant.userId),
              ),
              width: 300,
              height: 500,
              color: Colors.white,
              borderRadius: BorderRadius.circular(6),
            ),
            SearchableList(
              values: lottery.participants,
              onSearch: (allParticipants, searchTerm) {
                var regex = RegExp(searchTerm, caseSensitive: false);
                return allParticipants.where((p) => regex.hasMatch(p.walletAddress)).toList();
              },
              onDisplay: (participant) => ListTile(
                leading: CircleAvatar(
                  backgroundColor: Colors.green[200]!,
                  radius: 15,
                  child: Text(participant.l1BlockNumber.toString()),
                ),
                title: Text(participant.walletAddress),
                subtitle: Text(participant.userId),
              ),
              width: 300,
              height: 500,
              color: Colors.white,
              borderRadius: BorderRadius.circular(6),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildPollDialog(String title, PollVm poll) {
    return AlertDialog(
      title: Text(title),
      content: Column(
        children: [
          Text('InitBlockNumber: ${poll.initBlockNumber}'),
          SearchableList(
            values: poll.verifiers,
            onSearch: (allVerifiers, searchTerm) {
              var regex = RegExp(searchTerm, caseSensitive: false);
              return allVerifiers.where((v) => regex.hasMatch(v.walletAddress)).toList();
            },
            onDisplay: (verifier) => ListTile(
              title: Text(verifier.walletAddress),
              subtitle: Text(verifier.userId),
            ),
            width: 300,
            height: 500,
            color: Colors.white,
            borderRadius: BorderRadius.circular(6),
          ),
        ],
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
            Text('Subjects/Things/Proposals'),
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
            _buildSubjectsThingsProposalsTab(),
          ],
        ),
      ),
    );
  }
}
