// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

contract TestContract {
    mapping(string => string) private map_string_to_string;
    mapping(string => int16) private map_string_to_int16;
    mapping(string => bool) private map_string_to_bool;

    mapping(int64 => string) private map_int64_to_string;

    constructor() {
        map_string_to_string["short key 1"] = "short value";
        map_string_to_string[
            "short key 2"
        ] = "loooooooooooooooooooooooooooooooooooooooooooooooooooooooooong value";

        map_string_to_string[
            "loooooooooooooooooooooooooooooooooooooooooooooooooooooooooong key 1"
        ] = "short value";
        map_string_to_string[
            "loooooooooooooooooooooooooooooooooooooooooooooooooooooooooong key 2"
        ] = "loooooooooooooooooooooooooooooooooooooooooooooooooooooooooong value";

        map_string_to_int16["short key"] = 78;
        map_string_to_int16[
            "loooooooooooooooooooooooooooooooooooooooooooooooooooooooooong key"
        ] = -5996;

        map_string_to_bool["short key"] = false;
        map_string_to_bool[
            "loooooooooooooooooooooooooooooooooooooooooooooooooooooooooong key"
        ] = true;

        map_int64_to_string[7451684] = "short value";
        map_int64_to_string[
            -7451684
        ] = "loooooooooooooooooooooooooooooooooooooooooooooooooooooooooong value";
    }
}
