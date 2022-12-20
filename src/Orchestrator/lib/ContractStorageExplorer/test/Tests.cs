using Xunit.Abstractions;

using FluentAssertions;
using ContractStorageExplorer;
using ContractStorageExplorer.SolTypes;

public class Tests : IClassFixture<TestContract>
{
    private readonly ITestOutputHelper _output;

    private readonly Contract _contract;

    public Tests(ITestOutputHelper output, TestContract testContract)
    {
        _output = output;
        _contract = ContractFinder.Create()
            .WithLayoutDirectory("./")
            .WithName("TestContract")
            .DeployedAt(testContract.Address)
            .OnNetwork("http://localhost:8545/")
            .Find();
    }

    [Fact]
    public async Task ShouldBeBe()
    {
        var value = await _contract
            .WalkStorage()
            .Field("map_string_to_string")
            .AsMapping()
            .Key(new SolString("short key 1"))
            .GetValue<SolString>();

        value.Value.Should().Be("short value");

        value = await _contract
            .WalkStorage()
            .Field("map_string_to_string")
            .AsMapping()
            .Key(new SolString("short key 2"))
            .GetValue<SolString>();

        value.Value.Should().Be("loooooooooooooooooooooooooooooooooooooooooooooooooooooooooong value");

        value = await _contract
            .WalkStorage()
            .Field("map_string_to_string")
            .AsMapping()
            .Key(new SolString("loooooooooooooooooooooooooooooooooooooooooooooooooooooooooong key 1"))
            .GetValue<SolString>();

        value.Value.Should().Be("short value");

        value = await _contract
            .WalkStorage()
            .Field("map_string_to_string")
            .AsMapping()
            .Key(new SolString("loooooooooooooooooooooooooooooooooooooooooooooooooooooooooong key 2"))
            .GetValue<SolString>();

        value.Value.Should().Be("loooooooooooooooooooooooooooooooooooooooooooooooooooooooooong value");

        value = await _contract
            .WalkStorage()
            .Field("map_int64_to_string")
            .AsMapping()
            .Key(new SolInt64(7451684))
            .GetValue<SolString>();

        value.Value.Should().Be("short value");

        value = await _contract
            .WalkStorage()
            .Field("map_int64_to_string")
            .AsMapping()
            .Key(new SolInt64(-7451684))
            .GetValue<SolString>();

        value.Value.Should().Be("loooooooooooooooooooooooooooooooooooooooooooooooooooooooooong value");

        var boolValue = await _contract
            .WalkStorage()
            .Field("map_string_to_bool")
            .AsMapping()
            .Key(new SolString("short key"))
            .GetValue<SolBool>();

        boolValue.Value.Should().BeFalse();

        boolValue = await _contract
            .WalkStorage()
            .Field("map_string_to_bool")
            .AsMapping()
            .Key(new SolString("loooooooooooooooooooooooooooooooooooooooooooooooooooooooooong key"))
            .GetValue<SolBool>();

        boolValue.Value.Should().BeTrue();

        var int64Value = await _contract
            .WalkStorage()
            .Field("map_uint8_to_struct_a")
            .AsMapping()
            .Key(new SolUint8(5))
            .AsStruct("A")
            .Field("map_uint64_to_int64")
            .AsMapping()
            .Key(new SolUint64(15))
            .GetValue<SolInt64>();

        int64Value.Value.Should().Be(9633);

        value = await _contract
            .WalkStorage()
            .Field("map_uint8_to_struct_a")
            .AsMapping()
            .Key(new SolUint8(5))
            .AsStruct("A")
            .Field("name")
            .GetValue<SolString>();

        value.Value.Should().Be("Martin Martin Martin Martin Martin Martin Martin Martin Martin Sebastian Marcus Alex");

        var bytes16Value = await _contract
            .WalkStorage()
            .Field("map_b16_to_b16")
            .AsMapping()
            .Key(new SolBytes16(Convert.FromHexString("6F353500BBB93342A15967ABD0C6CA7B")))
            .GetValue<SolBytes16>();

        bytes16Value.HexValue.ToUpper().Should().Be("DF97506F262B0544B3FFAC8A70940CC8");

        bytes16Value = await _contract
            .WalkStorage()
            .Field("map_b16_to_b16")
            .AsMapping()
            .Key(new SolBytes16(Convert.FromHexString("E08A283DECFFB54785FDB9117A707E52")))
            .GetValue<SolBytes16>();

        bytes16Value.HexValue.ToUpper().Should().Be("DA87ADAB336C77428F0E6B84E4BDE406");

        value = await _contract
            .WalkStorage()
            .Field("arr_of_arr_of_arr_of_string")
            .AsArrayOf<SolArray>()
            .Index(14)
            .AsArrayOf<SolArray>()
            .Index(3)
            .AsArrayOf<SolString>()
            .Index(98)
            .GetValue<SolString>();

        value.Value.Should().Be("veeeeeeeeeeeeeeeeeeeeeeery loooooooooooooooooooooooooooooooooooooooooooooooooooooooooong value");

        var int8Value = await _contract
            .WalkStorage()
            .Field("arr_of_struct_b")
            .AsArrayOfStruct("B")
            .Index(0)
            .AsStruct("B")
            .Field("age")
            .GetValue<SolInt8>();

        int8Value.Value.Should().Be(15);

        value = await _contract
            .WalkStorage()
            .Field("arr_of_struct_b")
            .AsArrayOfStruct("B")
            .Index(1)
            .AsStruct("B")
            .Field("name")
            .GetValue<SolString>();

        value.Value.Should().Be("Bumblebee");

        int8Value = await _contract
            .WalkStorage()
            .Field("arr_of_struct_b")
            .AsArrayOfStruct("B")
            .Index(1)
            .AsStruct("B")
            .Field("age")
            .GetValue<SolInt8>();

        int8Value.Value.Should().Be(-122);

        boolValue = await _contract
            .WalkStorage()
            .Field("arr_of_struct_b")
            .AsArrayOfStruct("B")
            .Index(1)
            .AsStruct("B")
            .Field("yes")
            .GetValue<SolBool>();

        boolValue.Value.Should().BeTrue();

        value = await _contract
            .WalkStorage()
            .Field("map_b12_to_string")
            .AsMapping()
            .Key(new SolBytes12(Convert.FromHexString("6F353500BBB93342A15967AB")))
            .GetValue<SolString>();

        value.Value.Should().Be("good bye!");

        var bytes12Value = await _contract
            .WalkStorage()
            .Field("struct_c")
            .AsStruct("C")
            .Field("data")
            .GetValue<SolBytes12>();

        bytes12Value.HexValue.ToUpper().Should().Be("6F353500BBB93342A15967AB");

        var bytes32Value = await _contract
            .WalkStorage()
            .Field("struct_c")
            .AsStruct("C")
            .Field("dataBig")
            .GetValue<SolBytes32>();

        bytes32Value.Value.Should().Equal(new byte[32]);
    }
}