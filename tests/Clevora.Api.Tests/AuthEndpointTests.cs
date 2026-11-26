using Clevora.Shared.Models;
using Xunit;

namespace Clevora.Api.Tests;

public class AuthEndpointTests
{
    [Fact]
    public void RegisterRequest_AcceptsUsernameOrEmail()
    {
        var reqEmail = new RegisterRequest("user@clevora.com", "Password123!", "User Demo");
        var reqUsername = new RegisterRequest("kang_fadhil", "Password123!", "Kang Fadhil");
        var reqId = new RegisterRequest("acc-9812", "Password123!", "Akun 9812");

        Assert.Equal("user@clevora.com", reqEmail.Email);
        Assert.Equal("kang_fadhil", reqUsername.Email);
        Assert.Equal("acc-9812", reqId.Email);
    }

    [Fact]
    public void LoginRequest_AcceptsUsernameOrIdOrEmail()
    {
        var req1 = new LoginRequest("fadhil@gravicode.com", "Password123!");
        var req2 = new LoginRequest("fadhil", "Password123!");
        var req3 = new LoginRequest("USR-001", "Password123!");

        Assert.Equal("fadhil@gravicode.com", req1.Email);
        Assert.Equal("fadhil", req2.Email);
        Assert.Equal("USR-001", req3.Email);
    }

    [Fact]
    public void AuthResponse_MaintainsUserIdentityContract()
    {
        var res = new AuthResponse("token-xyz", "refresh-abc", "uid-100", "fadhil@gravicode.com", "Kang Fadhil");

        Assert.Equal("token-xyz", res.Token);
        Assert.Equal("refresh-abc", res.RefreshToken);
        Assert.Equal("uid-100", res.UserId);
        Assert.Equal("fadhil@gravicode.com", res.Email);
        Assert.Equal("Kang Fadhil", res.DisplayName);
    }
}
