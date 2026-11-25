namespace Clevora.Shared.Models;

public record RegisterRequest(
    string Email,
    string Password,
    string? DisplayName);

public record LoginRequest(
    string Email,
    string Password);

public record RefreshTokenRequest(
    string Token,
    string RefreshToken);

public record AuthResponse(
    string Token,
    string RefreshToken,
    string UserId,
    string Email,
    string? DisplayName);

public record UserProfileDto(
    string UserId,
    string Email,
    string? DisplayName,
    DateTime MemberSince);
