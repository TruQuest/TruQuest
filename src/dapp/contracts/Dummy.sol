// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

contract Dummy {
    string private s_value;

    function setValue(string calldata _value) external {
        s_value = _value;
    }

    function getValue() external view returns (string memory) {
        return s_value;
    }
}
