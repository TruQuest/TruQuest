// SPDX-License-Identifier: AGPL-3.0-only
pragma solidity >=0.8.0 <0.9.0;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";

error Truthserum__Unauthorized();

// @@NOTE: Just a dummy implementation for testing purposes until
// a proper token design and distribution strategy are decided upon.
contract Truthserum is ERC20 {
    address private immutable i_orchestrator;
    address private s_truQuestAddress;

    modifier onlyOrchestrator() {
        if (msg.sender != i_orchestrator) {
            revert Truthserum__Unauthorized();
        }
        _;
    }

    modifier onlyTruQuest() {
        if (msg.sender != s_truQuestAddress) {
            revert Truthserum__Unauthorized();
        }
        _;
    }

    constructor() ERC20("Truthserum", "TRU") {
        i_orchestrator = msg.sender;
    }

    function setTruQuestAddress(
        address _truQuestAddress
    ) external onlyOrchestrator {
        s_truQuestAddress = _truQuestAddress;
    }

    function decimals() public pure override returns (uint8) {
        return 9; // 1 Truthserum == 1_000_000_000 Guttae [guht-ee] (plural form of Gutta [guht-uh])
    }

    function mintTo(address _to, uint256 _amount) external onlyOrchestrator {
        _mint(_to, _amount);
    }

    function mint(uint256 _amount) external onlyTruQuest {
        _mint(msg.sender, _amount);
    }
}
