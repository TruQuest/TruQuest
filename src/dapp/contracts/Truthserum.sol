// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";

contract Truthserum is ERC20 {
    constructor() ERC20("Truthserum", "TRU") {
        _mint(msg.sender, 100000);

        _transfer(
            msg.sender,
            0xDF8DB8D4CeBC202cc4f5F064E08049C5228fC237,
            20000
        );

        address[8] memory senders = [
            0x69c2ac462AeeD245Fd1A92C789A5d6ccf94b05B7,
            0xd5938750a90d2B1529bE082dF1030882DEF5dBab,
            0x334A60c06D394Eef6970A0A6679DDbE767972FeD,
            0xcaF234cCb63cd528Aeb67Be009230f7a81563E7a,
            0x81d7125E7EF2ada9171904760D081cc08510C865,
            0x5d6E95D3b671aC27cacB2E8E61c3EC23f9C226EC,
            0x6105C4b563E975AF7E814f31b4f900f0129919e9,
            0x2a171e640EECA4e9DF7985eB8a80a19b3a0b6276
        ];

        for (uint256 i = 0; i < senders.length; ++i) {
            _transfer(msg.sender, senders[i], 1000);
            _approve(
                senders[i],
                0xDF8DB8D4CeBC202cc4f5F064E08049C5228fC237,
                500
            );
        }
    }
}
