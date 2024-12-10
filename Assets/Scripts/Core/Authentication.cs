using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;

public static class Authentication
{
    public static bool IsAuthenticated { get; private set; }
    public static string PlayerName { get; private set; }

    public static string PlayerId => AuthenticationService.Instance.PlayerId;

    public static async Task<bool> AuthenticateAsync(string playerName)
    {
        if (IsAuthenticated)
            return false;

        PlayerName = playerName;

        InitializationOptions initializationOptions = new();
        initializationOptions.SetProfile(PlayerName);

        await UnityServices.InitializeAsync(initializationOptions);

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        IsAuthenticated = true;

        return true;
    }
}