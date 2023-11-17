import 'dart:math';

import 'package:flutter/material.dart';
import 'package:animated_text_kit/animated_text_kit.dart';
import 'package:auto_size_text/auto_size_text.dart';
import 'package:flutter_card_swiper/flutter_card_swiper.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:rounded_loading_button/rounded_loading_button.dart';

import '../../general/contexts/document_context.dart';
import '../../general/contexts/page_context.dart';
import '../../general/widgets/corner_banner.dart';
import '../../general/widgets/document_composer.dart';
import '../../general/widgets/evidence_block.dart';
import '../../general/widgets/image_block_with_crop.dart';
import '../../general/widgets/restrict_when_unauthorized_button.dart';
import '../../settlement/bloc/settlement_actions.dart';
import '../../settlement/bloc/settlement_bloc.dart';
import '../../settlement/models/rvm/settlement_proposal_state_vm.dart';
import '../../settlement/models/rvm/verdict_vm.dart';
import '../../settlement/widgets/verdict_selection_block.dart';
import '../../widget_extensions.dart';
import '../bloc/thing_actions.dart';
import '../bloc/thing_bloc.dart';
import '../models/rvm/settlement_proposal_preview_vm.dart';
import '../../general/utils/utils.dart';

class SettlementProposalsList extends StatefulWidget {
  final String thingId;

  const SettlementProposalsList({super.key, required this.thingId});

  @override
  State<SettlementProposalsList> createState() => _SettlementProposalsListState();
}

class _SettlementProposalsListState extends StateX<SettlementProposalsList> {
  late final _pageContext = use<PageContext>();
  late final _thingBloc = use<ThingBloc>();
  late final _settlementBloc = use<SettlementBloc>();

  @override
  void initState() {
    super.initState();
    _thingBloc.dispatch(GetSettlementProposalsList(thingId: widget.thingId));
  }

  @override
  Widget buildX(BuildContext context) {
    return Container(
      width: double.infinity,
      height: double.infinity,
      decoration: BoxDecoration(
        color: const Color(0xFF242423),
        borderRadius: const BorderRadius.only(
          topLeft: Radius.circular(16),
          topRight: Radius.circular(16),
        ),
      ),
      child: StreamBuilder(
        stream: _thingBloc.proposalsList$,
        builder: (context, snapshot) {
          if (snapshot.data == null) return Center(child: CircularProgressIndicator());

          var proposals = snapshot.data!;
          var highlightedProposal = proposals
              .where(
                (p) =>
                    p.state == SettlementProposalStateVm.fundedAndVerifierLotteryInitiated ||
                    p.state == SettlementProposalStateVm.verifiersSelectedAndPollInitiated ||
                    p.state == SettlementProposalStateVm.accepted,
              )
              .firstOrNull;
          var otherProposals = proposals
              .where(
                (p) =>
                    p.state == SettlementProposalStateVm.draft || p.state == SettlementProposalStateVm.awaitingFunding,
              )
              .toList();
          var archivedProposals = proposals
              .where(
                (p) =>
                    p.state == SettlementProposalStateVm.verifierLotteryFailed ||
                    p.state == SettlementProposalStateVm.consensusNotReached ||
                    p.state == SettlementProposalStateVm.declined,
              )
              .toList();

          return Column(
            children: [
              const SizedBox(height: 48),
              Row(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  Container(
                    margin: const EdgeInsets.only(left: 20),
                    color: Colors.white,
                    width: 300,
                    padding: const EdgeInsets.all(8),
                    child: DefaultTextStyle(
                      style: GoogleFonts.righteous(
                        fontSize: 24,
                        color: Colors.black,
                      ),
                      child: Row(
                        children: [
                          const Text('> '),
                          AnimatedTextKit(
                            repeatForever: true,
                            pause: const Duration(seconds: 2),
                            animatedTexts: [
                              TypewriterAnimatedText(
                                highlightedProposal == null
                                    ? 'No active proposal'
                                    : highlightedProposal.state == SettlementProposalStateVm.accepted
                                        ? 'Accepted proposal'
                                        : 'Proposal under assessment',
                                speed: const Duration(milliseconds: 70),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                  ),
                  Spacer(),
                  ElevatedButton.icon(
                    style: ElevatedButton.styleFrom(
                      backgroundColor: const Color(0xffF8F9FA),
                      foregroundColor: const Color(0xFF242423),
                      elevation: 10,
                    ),
                    icon: const Icon(Icons.search),
                    label: const Text('Search'),
                    onPressed: () {},
                  ),
                  const SizedBox(width: 12),
                  RestrictWhenUnauthorizedButton(
                    child: ElevatedButton.icon(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: const Color(0xffF8F9FA),
                        foregroundColor: const Color(0xFF242423),
                        elevation: 10,
                      ),
                      icon: const Icon(Icons.add),
                      label: const Text('Add'),
                      onPressed: () {
                        var documentContext = DocumentContext();
                        documentContext.thingId = widget.thingId;

                        var btnController = RoundedLoadingButtonController();

                        showDialog(
                          context: context,
                          barrierDismissible: false,
                          builder: (context) => ScopeX(
                            useInstances: [documentContext],
                            child: DocumentComposer(
                              title: 'New settlement proposal',
                              nameFieldLabel: 'Title',
                              submitButton: Padding(
                                padding: const EdgeInsets.symmetric(horizontal: 12),
                                child: RoundedLoadingButton(
                                  child: const Text('Prepare draft'),
                                  controller: btnController,
                                  onPressed: () async {
                                    var success = await _settlementBloc.execute<bool>(
                                      CreateNewSettlementProposalDraft(
                                        documentContext: DocumentContext.fromEditable(documentContext),
                                      ),
                                    );

                                    if (!success.isTrue) {
                                      btnController.error();
                                      await Future.delayed(const Duration(milliseconds: 1500));
                                      btnController.reset();

                                      return;
                                    }

                                    btnController.success();
                                    await Future.delayed(const Duration(milliseconds: 1500));
                                    if (context.mounted) Navigator.of(context).pop();
                                  },
                                ),
                              ),
                              sideBlocks: const [
                                VerdictSelectionBlock(),
                                ImageBlockWithCrop(cropCircle: false),
                                EvidenceBlock(),
                              ],
                            ),
                          ),
                        );
                      },
                    ),
                  ),
                  const SizedBox(width: 20),
                ],
              ),
              SizedBox(height: 16),
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Expanded(
                    flex: 3,
                    child: Card(
                      margin: const EdgeInsets.fromLTRB(20, 0, 20, 16),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(10),
                      ),
                      clipBehavior: Clip.antiAlias,
                      child: Stack(
                        children: [
                          Column(
                            children: [
                              highlightedProposal == null
                                  ? Container(
                                      height: 400,
                                      padding: const EdgeInsets.symmetric(vertical: 75),
                                      child: Image.asset(
                                        'assets/images/document.png',
                                        width: double.infinity,
                                        fit: BoxFit.scaleDown,
                                      ),
                                    )
                                  : Image.network(
                                      '${dotenv.env['IPFS_GATEWAY_URL']}/${highlightedProposal.croppedImageIpfsCid!}',
                                      width: double.infinity,
                                      height: 400,
                                      fit: BoxFit.cover,
                                    ),
                              Container(
                                width: double.infinity,
                                height: 90,
                                decoration: BoxDecoration(
                                  color: const Color(0xffF8F9FA),
                                  border: const Border(
                                    top: BorderSide(
                                      color: Colors.black,
                                      width: 3,
                                    ),
                                  ),
                                ),
                                alignment: Alignment.bottomLeft,
                                padding: const EdgeInsets.fromLTRB(8, 0, 0, 10),
                                child: Text(
                                  highlightedProposal == null
                                      ? '> [404] Not Found'
                                      : '> [Verdict] ${highlightedProposal.verdict.getString()}',
                                  style: GoogleFonts.righteous(
                                    fontSize: 24,
                                    color: Colors.black,
                                  ),
                                ),
                              ),
                            ],
                          ),
                          Positioned(
                            top: 365,
                            left: 4,
                            right: 120,
                            height: 70,
                            child: GestureDetector(
                              onTap: highlightedProposal != null
                                  ? () => _pageContext.goto('/proposals/${highlightedProposal.id}')
                                  : null,
                              child: Card(
                                color: const Color(0xFF242423),
                                shadowColor: Colors.black,
                                elevation: 5,
                                shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(6),
                                ),
                                child: highlightedProposal == null
                                    ? Container(
                                        padding: const EdgeInsets.only(left: 4),
                                        alignment: Alignment.centerLeft,
                                        child: _buildPlaceholderTitle(20, 6),
                                      )
                                    : Row(
                                        children: [
                                          Expanded(
                                            child: Container(
                                              padding: const EdgeInsets.symmetric(horizontal: 12),
                                              alignment: Alignment.centerLeft,
                                              child: AutoSizeText(
                                                highlightedProposal.title,
                                                style: GoogleFonts.philosopher(
                                                  color: Colors.white,
                                                  fontSize: 24,
                                                ),
                                                maxLines: 1,
                                                overflow: TextOverflow.ellipsis,
                                              ),
                                            ),
                                          ),
                                          Container(
                                            width: 25,
                                            decoration: BoxDecoration(
                                              color: Colors.white,
                                              borderRadius: const BorderRadius.only(
                                                topRight: Radius.circular(6),
                                                bottomRight: Radius.circular(6),
                                              ),
                                            ),
                                            alignment: Alignment.center,
                                            child: Icon(
                                              Icons.arrow_forward_ios,
                                              size: 18,
                                            ),
                                          ),
                                        ],
                                      ),
                              ),
                            ),
                          ),
                          if (highlightedProposal != null)
                            Positioned(
                              top: 0,
                              right: 0,
                              child: Tooltip(
                                message: highlightedProposal.state.getString(),
                                child: CornerBanner(
                                  position: Alignment.topRight,
                                  size: 50,
                                  color: Colors.black,
                                  cornerRadius: 10,
                                  child: Icon(
                                    highlightedProposal.stateIcon,
                                    size: 16,
                                    color: Colors.white,
                                  ),
                                ),
                              ),
                            ),
                        ],
                      ),
                    ),
                  ),
                  Expanded(
                    child: Column(
                      children: [
                        SizedBox(
                          height: 320,
                          child: Row(
                            children: [
                              RotatedBox(
                                quarterTurns: 3,
                                child: Padding(
                                  padding: const EdgeInsets.only(left: 16),
                                  child: Text(
                                    'Other',
                                    style: GoogleFonts.righteous(
                                      color: Colors.white,
                                      fontSize: 24,
                                    ),
                                  ),
                                ),
                              ),
                              Expanded(
                                child: CardSwiper(
                                  padding: const EdgeInsets.fromLTRB(8, 0, 16, 0),
                                  isDisabled: otherProposals.length <= 1,
                                  cardBuilder: (context, index) {
                                    var proposal = index < otherProposals.length ? otherProposals[index] : null;
                                    return _buildProposalCard(proposal);
                                  },
                                  cardsCount: max(otherProposals.length, 1),
                                  numberOfCardsDisplayed: max(otherProposals.length, 1),
                                ),
                              ),
                            ],
                          ),
                        ),
                        SizedBox(
                          height: 320,
                          child: Row(
                            children: [
                              RotatedBox(
                                quarterTurns: 3,
                                child: Padding(
                                  padding: const EdgeInsets.only(left: 16),
                                  child: Text(
                                    'Archived',
                                    style: GoogleFonts.righteous(
                                      color: Colors.white,
                                      fontSize: 24,
                                    ),
                                  ),
                                ),
                              ),
                              Expanded(
                                child: CardSwiper(
                                  padding: const EdgeInsets.fromLTRB(8, 0, 16, 0),
                                  isDisabled: archivedProposals.length <= 1,
                                  cardBuilder: (context, index) {
                                    var proposal = index < archivedProposals.length ? archivedProposals[index] : null;
                                    return _buildProposalCard(proposal);
                                  },
                                  cardsCount: max(archivedProposals.length, 1),
                                  numberOfCardsDisplayed: max(archivedProposals.length, 1),
                                ),
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ],
          );
        },
      ),
    );
  }

  Widget _buildProposalCard(SettlementProposalPreviewVm? proposal) {
    return Card(
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(10),
      ),
      clipBehavior: Clip.antiAlias,
      child: Stack(
        children: [
          SizedBox(
            height: 260,
            child: Column(
              children: [
                proposal == null
                    ? Container(
                        height: 200,
                        padding: const EdgeInsets.symmetric(vertical: 50),
                        child: Image.asset(
                          'assets/images/document.png',
                          width: double.infinity,
                          fit: BoxFit.scaleDown,
                        ),
                      )
                    : Image.network(
                        '${dotenv.env['IPFS_GATEWAY_URL']}/${proposal.croppedImageIpfsCid!}',
                        width: double.infinity,
                        height: 200,
                        fit: BoxFit.cover,
                      ),
                Expanded(
                  child: Container(
                    width: double.infinity,
                    decoration: BoxDecoration(
                      color: const Color(0xffF8F9FA),
                      border: const Border(
                        top: BorderSide(
                          color: Colors.black,
                          width: 2,
                        ),
                      ),
                    ),
                    alignment: Alignment.bottomLeft,
                    padding: const EdgeInsets.fromLTRB(8, 0, 0, 10),
                    child: Text(
                      proposal == null ? '> [404] Not Found' : '> ${proposal.verdict.getString()}',
                      style: GoogleFonts.righteous(
                        fontSize: 16,
                        color: Colors.black,
                      ),
                    ),
                  ),
                ),
              ],
            ),
          ),
          Positioned(
            top: 180,
            left: 2,
            right: 12,
            height: 40,
            child: GestureDetector(
              onTap: proposal != null ? () => _pageContext.goto('/proposals/${proposal.id}') : null,
              child: Card(
                color: const Color(0xFF242423),
                shadowColor: Colors.black,
                elevation: 3,
                child: proposal == null
                    ? Container(
                        padding: const EdgeInsets.only(left: 4),
                        alignment: Alignment.centerLeft,
                        child: _buildPlaceholderTitle(8, 4),
                      )
                    : Container(
                        padding: const EdgeInsets.symmetric(horizontal: 6),
                        alignment: Alignment.centerLeft,
                        child: AutoSizeText(
                          proposal.title,
                          style: GoogleFonts.philosopher(
                            color: Colors.white,
                            fontSize: 16,
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
              ),
            ),
          ),
          if (proposal != null)
            Positioned(
              top: 0,
              right: 0,
              child: Tooltip(
                message: proposal.state.getString(),
                child: CornerBanner(
                  position: Alignment.topRight,
                  size: 34,
                  color: Colors.black,
                  cornerRadius: 10,
                  child: Icon(
                    proposal.stateIcon,
                    size: 11,
                    color: Colors.white,
                  ),
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildPlaceholderTitle(double lineHeight, double gap) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Row(
          children: [
            Expanded(
              flex: 9,
              child: Container(
                height: lineHeight,
                decoration: BoxDecoration(
                  color: const Color(0xfffae7e3),
                  borderRadius: BorderRadius.circular(6),
                ),
              ),
            ),
            const Spacer(),
          ],
        ),
        SizedBox(height: gap),
        Row(
          children: [
            Expanded(
              flex: 6,
              child: Container(
                height: lineHeight,
                decoration: BoxDecoration(
                  color: const Color(0xfffae7e3),
                  borderRadius: BorderRadius.circular(6),
                ),
              ),
            ),
            const Spacer(flex: 4),
          ],
        ),
      ],
    );
  }
}
