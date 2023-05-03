using System.Text.Json;
using WebApplicationSchedulePlannerMVC.Extensions;

namespace WebApplicationSchedulePlannerMVC.NamingPolicies;

public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) =>
            name.ToSnakeCase();
}