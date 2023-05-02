using System.Text.Json;
using WebApplicationSchedulePlanner.Extensions;

namespace WebApplicationSchedulePlanner.NamingPolices;

public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) =>
            name.ToSnakeCase();
}