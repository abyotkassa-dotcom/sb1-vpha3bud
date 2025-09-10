using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using CMTEngTaskManagement.Shared.DTOs;
using Blazored.LocalStorage;

namespace CMTEngTaskManagement.Client.Services
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task LogoutAsync();
        Task<UserDto?> GetCurrentUserAsync();
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthenticationService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    
                    if (loginResponse?.Success == true && !string.IsNullOrEmpty(loginResponse.Token))
                    {
                        await _localStorage.SetItemAsync("authToken", loginResponse.Token);
                        await _localStorage.SetItemAsync("user", loginResponse.User);
                        
                        // Update authentication state
                        ((CustomAuthStateProvider)_authStateProvider).MarkUserAsAuthenticated(loginResponse.User!);
                        
                        return loginResponse;
                    }
                }
                
                var errorResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return errorResponse ?? new LoginResponse { Success = false, Message = "Login failed." };
            }
            catch (Exception ex)
            {
                return new LoginResponse { Success = false, Message = "Network error occurred." };
            }
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("user");
            ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
        }

        public async Task<UserDto?> GetCurrentUserAsync()
        {
            try
            {
                return await _localStorage.GetItemAsync<UserDto>("user");
            }
            catch
            {
                return null;
            }
        }
    }

    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _httpClient;

        public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient httpClient)
        {
            _localStorage = localStorage;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            var user = await _localStorage.GetItemAsync<UserDto>("user");

            if (string.IsNullOrEmpty(token) || user == null)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Set Authorization header
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            if (user.ShopId.HasValue)
            {
                claims.Add(new Claim("ShopId", user.ShopId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            var principal = new ClaimsPrincipal(identity);

            return new AuthenticationState(principal);
        }

        public void MarkUserAsAuthenticated(UserDto user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            if (user.ShopId.HasValue)
            {
                claims.Add(new Claim("ShopId", user.ShopId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            var principal = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }

        public void MarkUserAsLoggedOut()
        {
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }
    }
}