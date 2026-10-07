using BackendApi.DTOs.Responses;
using BackendApi.Entities;

namespace BackendApi.Mappers;

public static class AlertMapper
{
    public static AlertResponse ToResponse(Alert alert) => new(
        alert.Id,
        alert.EventId,
        alert.SourceEventId,
        alert.RuleId,
        alert.PropertyId,
        alert.PropertyName,
        alert.Variable,
        alert.Operator,
        alert.Threshold,
        alert.MeasuredValue,
        alert.Severity,
        alert.ObservedAt,
        alert.CreatedAt);
}
