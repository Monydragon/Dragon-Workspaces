using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dragon_Workspaces.Data.Persistence;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false)]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal sealed partial class WorkspaceJsonSerializerContext : JsonSerializerContext;
