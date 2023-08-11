// SPDX-License-Identifier: AGPL-3.0-only
pragma solidity >=0.8.0 <0.9.0;

contract L1Block {
    /**
     * @notice Address of the special depositor account.
     */
    address public constant DEPOSITOR_ACCOUNT =
        0xDeaDDEaDDeAdDeAdDEAdDEaddeAddEAdDEAd0001;

    /**
     * @notice The latest L1 block number known by the L2 system.
     */
    uint64 public number;

    /**
     * @notice The latest L1 timestamp known by the L2 system.
     */
    uint64 public timestamp;

    /**
     * @notice The latest L1 basefee.
     */
    uint256 public basefee;

    /**
     * @notice The latest L1 blockhash.
     */
    bytes32 public hash;

    /**
     * @notice The number of L2 blocks in the same epoch.
     */
    uint64 public sequenceNumber;

    /**
     * @notice The versioned hash to authenticate the batcher by.
     */
    bytes32 public batcherHash;

    /**
     * @notice The overhead value applied to the L1 portion of the transaction
     *         fee.
     */
    uint256 public l1FeeOverhead;

    /**
     * @notice The scalar value applied to the L1 portion of the transaction fee.
     */
    uint256 public l1FeeScalar;
}
