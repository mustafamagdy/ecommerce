namespace FSH.WebApi.Application.Identity.Tokens;

public record TokenResponse(string Token, string RefreshToken, DateTime RefreshTokenExpiryTime);

public record OverrideTokenResponse(string Token);