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
    }
}