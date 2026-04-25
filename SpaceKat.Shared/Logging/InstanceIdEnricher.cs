using Serilog.Core;
using Serilog.Events;

namespace SpaceKat.Shared.Logging;

/// <summary>
/// Serilog enricher that adds an InstanceId property to all log events.
/// This allows different application instances to be distinguished in logs and OpenTelemetry.
/// </summary>
public class InstanceIdEnricher : ILogEventEnricher
{
    private readonly string _instanceId;

    /// <summary>
    /// Initializes a new instance of the InstanceIdEnricher class.
    /// </summary>
    /// <param name="instanceId">The unique identifier for this application instance.</param>
    public InstanceIdEnricher(string instanceId)
    {
        _instanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
    }

    /// <summary>
    /// Enriches the log event with the InstanceId property.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">Factory for creating log event properties.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("InstanceId", _instanceId));
    }
}
