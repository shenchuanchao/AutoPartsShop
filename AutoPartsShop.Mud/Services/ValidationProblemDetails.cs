using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AutoPartsShop.Mud.Services;

internal class ValidationProblemDetails : ProblemDetails
{
    [JsonPropertyName("errors")]
    public Dictionary<string, List<string>>? Errors { get; set; }
}
