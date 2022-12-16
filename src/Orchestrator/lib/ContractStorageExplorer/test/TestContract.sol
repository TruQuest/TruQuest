// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

contract TestContract {
    struct A {
        mapping(uint64 => int64) map_uint64_to_int64;
        string name;
    }

    struct B {
        string name;
        int8 age;
        bool yes;
    }

    mapping(string => string) private map_string_to_string;
    mapping(string => int16) private map_string_to_int16;
    mapping(string => bool) private map_string_to_bool;

    mapping(int64 => string) private map_int64_to_string;

    mapping(uint8 => A) private map_uint8_to_struct_a;

    string[][][] private arr_of_arr_of_arr_of_string;
    B[] private arr_of_struct_b;

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

        A storage a = map_uint8_to_struct_a[5];
        a.map_uint64_to_int64[15] = 9633;
        a
            .name = "Martin Martin Martin Martin Martin Martin Martin Martin Martin Sebastian Marcus Alex";

        arr_of_arr_of_arr_of_string = new string[][][](20);
        arr_of_arr_of_arr_of_string[14] = new string[][](5);
        arr_of_arr_of_arr_of_string[14][3] = new string[](99);
        arr_of_arr_of_arr_of_string[14][3][
            98
        ] = "veeeeeeeeeeeeeeeeeeeeeeery loooooooooooooooooooooooooooooooooooooooooooooooooooooooooong value";

        arr_of_struct_b.push(B("McAllister", 15, false));
        arr_of_struct_b.push(B("Bumblebee", -122, true));
    }
}
