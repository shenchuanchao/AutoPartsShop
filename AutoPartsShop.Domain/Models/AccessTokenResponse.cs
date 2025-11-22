using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPartsShop.Domain.Models;

public sealed class AccessTokenResponse
{
    public string TokenType { get; } = "Bearer";

    public required string AccessToken { get; init; }

    public required long ExpiresIn { get; init; }

    public required string RefreshToken { get; init; }
}