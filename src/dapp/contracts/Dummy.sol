// SPDX-License-Identifier: AGPL-3.0-only
pragma solidity >=0.8.0 <0.9.0;

error Dummy__FooFoo(uint16 age);
error Dummy__MooMoo(uint16 age);

contract Dummy {
    address private immutable i_owner;

    string public s_name;
    uint16 public s_age;

    event Fooed(string indexed name, uint16 age, bytes32 data);
    event Mooed(uint16 age);

    constructor() {
        i_owner = msg.sender;
    }

    function foo(string calldata name, uint16 age, bytes32 data) external {
        if (age > 20) {
            revert Dummy__FooFoo(age);
        }

        s_name = name;
        s_age = age;

        emit Fooed(name, age, data);
    }

    function moo(uint16 age) external {
        if (age > 10) {
            revert Dummy__MooMoo(age);
        }

        s_age = age;

        emit Mooed(age);
    }
}
