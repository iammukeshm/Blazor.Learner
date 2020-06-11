using Blazor.Learner.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;

namespace Blazor.Learner.Client.Services
{
public class CustomStateProvider : AuthenticationStateProvider
{
    private readonly IAuthService api;
    private CurrentUser _currentUser;
    public CustomStateProvider(IAuthService api)
    {
        this.api = api;
    }
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();
        try
        {
            var userInfo = await GetCurrentUser();
            if (userInfo.IsAuthenticated)
            {
                var claims = new[] { new Claim(ClaimTypes.Name, _currentUser.UserName) }.Concat(_currentUser.Claims.Select(c => new Claim(c.Key, c.Value)));
                identity = new ClaimsIdentity(claims, "Server authentication");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Request failed:" + ex.ToString());
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private async Task<CurrentUser> GetCurrentUser()
    {
        if (_currentUser != null && _currentUser.IsAuthenticated) return _currentUser;
        _currentUser = await api.CurrentUserInfo();
        return _currentUser;
    }
    public async Task Logout()
    {
        await api.Logout();
        _currentUser = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
    public async Task Login(LoginRequest loginParameters)
    {
        await api.Login(loginParameters);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
    public async Task Register(RegisterRequest registerParameters)
    {
        await api.Register(registerParameters);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
}
