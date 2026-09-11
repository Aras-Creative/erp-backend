using System.Text.Json.Serialization;
using ArasERP.Modules.Inventory.Application.Batches.Create;

namespace ArasERP.Modules.Inventory.Api.Request;

public sealed class CreateBatchRequest
{
    public required Guid ItemId { get; init; }
    public required Guid WarehouseId { get; init; }
    public required decimal ReceivedQty { get; init; }
    public required decimal UnitCost { get; init; }
    public required DateTime ReceivedAt { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required SourceTypeEnum SourceType { get; init; }

    public string? ExternalReferenceNo { get; init; }
    public string? Note { get; init; }
    public string? ReceivedBy { get; init; }
}