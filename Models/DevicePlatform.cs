using NpgsqlTypes;

namespace CarwashApi.Models;

public enum DevicePlatform
{
    [PgName("Android")]
    Android,

    [PgName("iOS")]
    Ios,

    [PgName("Web")]
    Web,
}