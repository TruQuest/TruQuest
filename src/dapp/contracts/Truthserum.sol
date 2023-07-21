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
            0x20FD69D46DC690ef926d209FF016398D6613F168,
            0x29b9B8924cD0c6eae70981f611f3A2a07AC61f16,
            0xFC2a6bE9D03eb0F4Db06EaBCac63be3f5002A09B,
            0x0aB37d130deD0a85fCf2d472ac7aef1650C3CaaE,
            0x881606962701F9483d1D5FAD45d48C27Ec9698E7,
            0xaB45E127Fd54B2302E0B1c76d0444b50E12D6d1B,
            0x297c19fb45f0a4022c6D7030f21696207e51B9B8,
            0x9914DADEe4De641Da1f124Fc6026535be249ECc8
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
