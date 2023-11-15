using System.Diagnostics.Metrics;
using System.Reflection;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application;

public static class Metrics
{
    public static IReadOnlyDictionary<string, Histogram<int>> FunctionNameToGasUsedHistogram { get; }

    public static Func<double> OnOrchestratorBalanceObserved
    {
        set
        {
            _orchestratorBalanceGauge = Telemetry.Meter.CreateObservableGauge<double>(
                name: "orchestrator.balance.in-ether",
                observeValue: value,
                unit: "ether",
                description: "Orchestrator balance on L2"
            );
        }
    }
    private static ObservableGauge<double> _orchestratorBalanceGauge;

    public static Func<int> OnDeadLetterCountObserved
    {
        set
        {
            _deadLetterCountGauge = Telemetry.Meter.CreateObservableGauge<int>(
                name: "dead-letter.count",
                observeValue: value,
                description: "Number of archived unhandled dead letters"
            );
        }
    }
    private static ObservableGauge<int> _deadLetterCountGauge;

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
