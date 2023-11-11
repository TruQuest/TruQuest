// SPDX-License-Identifier: AGPL-3.0-only
pragma solidity >=0.8.0 <0.9.0;

error RestrictedAccess__Unauthorized();
error RestrictedAccess__Forbidden();

contract RestrictedAccess {
    address private s_orchestrator;

    address[] private s_whitelist;
    mapping(address => bool) private s_whitelistedAddresses;

    modifier onlyOrchestrator() {
        if (msg.sender != s_orchestrator) {
            revert RestrictedAccess__Unauthorized();
        }
        _;
    }

    constructor() {
        s_orchestrator = msg.sender;
    }

    function getWhitelist() external view returns (address[] memory) {
        return s_whitelist;
    }

    function checkHasAccess(address _user) external view returns (bool) {
        return s_whitelistedAddresses[_user];
    }

    function giveAccessTo(address _user) external onlyOrchestrator {
        if (!s_whitelistedAddresses[_user]) {
            s_whitelist.push(_user);
            s_whitelistedAddresses[_user] = true;
        }
    }

    function giveAccessToMany(
        address[] calldata _users
    ) external onlyOrchestrator {
        // @@TODO: Optimize.
        for (uint16 i = 0; i < _users.length; ++i) {
            address user = _users[i];
            if (!s_whitelistedAddresses[user]) {
                s_whitelist.push(user);
                s_whitelistedAddresses[user] = true;
            }
        }
    }

    function removeAccessFrom(
        address _user,
        uint16 _whitelistIndex
    ) external onlyOrchestrator {
        if (
            _whitelistIndex < s_whitelist.length &&
            s_whitelist[_whitelistIndex] == _user
        ) {
            if (_whitelistIndex < s_whitelist.length - 1) {
                s_whitelist[_whitelistIndex] = s_whitelist[
                    s_whitelist.length - 1
                ];
            }
            s_whitelist.pop();
            delete s_whitelistedAddresses[_user];
        }
    }
}
