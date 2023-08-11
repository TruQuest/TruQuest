// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";

error Truthserum__Unauthorized();

// @@NOTE: Just a dummy implementation for testing purposes until
// a proper token design and distribution strategy are decided upon.
contract Truthserum is ERC20 {
    address private immutable i_orchestrator;

    modifier onlyOrchestrator() {
        if (msg.sender != i_orchestrator) {
            revert Truthserum__Unauthorized();
        }
        _;
    }

    constructor() ERC20("Truthserum", "TRU") {
        i_orchestrator = msg.sender;
    }

    function decimals() public pure override returns (uint8) {
        return 9; // 1 Truthserum == 1_000_000_000 Guttae [guht-ee] (plural form of Gutta [guht-uh])
    }

    function mintTo(address _to, uint256 _amount) external onlyOrchestrator {
        _mint(_to, _amount);
    }
}
