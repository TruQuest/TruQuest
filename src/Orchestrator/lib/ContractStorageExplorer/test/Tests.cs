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
    }
}