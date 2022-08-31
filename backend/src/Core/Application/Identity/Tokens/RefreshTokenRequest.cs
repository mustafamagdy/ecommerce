using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Application.Identity.Tokens;

public record RefreshTokenRequest(string Token, string RefreshToken, SubscriptionType Subscription);