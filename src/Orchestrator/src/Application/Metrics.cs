using System.Diagnostics.Metrics;
using System.Reflection;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application;

public static class Metrics
{
    public static IReadOnlyDictionary<string, Histogram<int>> FunctionNameToGasUsedHistogram { get; }

    private static Func<double> _onOrchestratorBalanceObserved;
    public static Func<double> OnOrchestratorBalanceObserved
    {
        set
        {
            _onOrchestratorBalanceObserved = value;
            _orchestratorBalanceGauge = Telemetry.Meter.CreateObservableGauge<double>(
                name: "orchestrator.balance.in-ether",
                observeValue: _onOrchestratorBalanceObserved,
                unit: "ether",
                description: "Orchestrator balance on L2"
            );
        }
    }

    private static ObservableGauge<double> _orchestratorBalanceGauge;

    static Metrics()
    {
        var functionNameToGasUsedHistogram = new Dictionary<string, Histogram<int>>();
        foreach (
            var method in typeof(IContractCaller)
                .GetMethods()
                .Where(m => m.GetCustomAttribute<TrackGasUsageAttribute>() != null)
        )
        {
            var attr = method.GetCustomAttribute<TrackGasUsageAttribute>()!;
            functionNameToGasUsedHistogram[method.Name] = Telemetry.Meter.CreateHistogram<int>(
                name: $"contract-call.{attr.MetricName}.gas-used",
                unit: "gas",
                description: "Gas used by transactions"
            );
        }

        FunctionNameToGasUsedHistogram = functionNameToGasUsedHistogram;
    }
}
