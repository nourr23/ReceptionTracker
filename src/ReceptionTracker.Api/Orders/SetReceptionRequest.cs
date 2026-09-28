namespace ReceptionTracker.Api.Orders;

/// <summary>
/// Body of the PUT …/reception endpoints.
/// <c>required</c> makes the JSON deserializer reject a body without "isReceived" (400)
/// instead of silently defaulting it to false and un-validating everything.
/// </summary>
public sealed record SetReceptionRequest
{
    public required bool IsReceived { get; init; }
}
