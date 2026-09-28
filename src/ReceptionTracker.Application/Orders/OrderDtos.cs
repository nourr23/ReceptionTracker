using ReceptionTracker.Domain.Orders;

namespace ReceptionTracker.Application.Orders;

// API contracts. They follow the payload suggested in the brief
// ("orderId", "pallets", "palletId", "cartonId", "ref"…) and add statuses and progress.

public sealed record ProgressDto(int ReceivedLines, int TotalLines, int ReceivedUnits, int TotalUnits);

public sealed record OrderSummaryDto(string OrderId, ReceptionStatus Status, ProgressDto Progress);

public sealed record OrderDto(
    string OrderId,
    ReceptionStatus Status,
    ProgressDto Progress,
    IReadOnlyList<PalletDto> Pallets);

public sealed record PalletDto(
    string PalletId,
    ReceptionStatus Status,
    ProgressDto Progress,
    IReadOnlyList<CartonDto> Cartons);

public sealed record CartonDto(
    string CartonId,
    ReceptionStatus Status,
    ProgressDto Progress,
    IReadOnlyList<ProductLineDto> Products);

public sealed record ProductLineDto(
    int Id,
    string Ref,
    string Name,
    string Color,
    string Size,
    int ExpectedQuantity,
    bool IsReceived);
