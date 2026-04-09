using NpgsqlTypes;

namespace CarwashApi.Models;

public enum DeliveryStatus
{
    [PgName("PENDING")]
    Pending,

    [PgName("SENT")]
    Sent,

    [PgName("FAILED")]
    Failed,
}
