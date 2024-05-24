using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DSR
{
    // Ensure that LoginPage is a partial class that extends ContentPage
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            this.BindingContext = new LoginViewModel();

            // Ensure the page is fully loaded before setting up navigation
            this.Appearing += async (sender, args) =>
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("///login", animate: true);
                }
                else
                {
                    Debug.WriteLine("Shell.Current is null in Appearing.");
                }
            };
        }

    }

    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _userName;
        private string _password;
        private bool _isAdmin;
        public ICommand LogInCommand { get; }
        public ICommand NavigateToSignUpCommand { get; }

        public ICommand GoogleLogInCommand { get; }


        public LoginViewModel()
        {
            LogInCommand = new Command(async () => await PerformLogin());
            NavigateToSignUpCommand = new Command(async () => await NavigateToSignUp());
            GoogleLogInCommand = new Command(async () => await PerformGoogleLogin());

        }

        public async Task PerformGoogleLogin()
        {
            int freePort = FindFreePort();
            string redirectUri = $"http://127.0.0.1:{freePort}/";

            try
            {
                // Start the authentication process
                var authResult = await WebAuthenticator.AuthenticateAsync(
                    new Uri($"https://accounts.google.com/o/oauth2/v2/auth?client_id=581305694892-cqb0t3lrf76n6pn2kcsl5276lcrubaqi.apps.googleusercontent.com&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope=email"),
                    new Uri(redirectUri));

                // Exchange code for token
                string code = authResult.Properties["code"];
                var token = await ExchangeCodeForToken(code, redirectUri);

                if (!string.IsNullOrEmpty(token))
                {
                    // Use the token to fetch user details or verify the user
                    string userEmail = await FetchEmailFromGoogle(token);
                    if (await DatabaseHelper.CheckUserEmailExistsAsync(userEmail))
                    {
                        await Shell.Current.GoToAsync("///dashboard");
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Login Failure", "No such user registered with Google email.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Authentication error: {ex}");
                await App.Current.MainPage.DisplayAlert("Authentication Error", $"An error occurred during Google login. Detailed error: {ex.Message}", "OK");
            }

        }

        private async Task<string> ExchangeCodeForToken(string code, string redirectUri)
        {
            using var httpClient = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string, string>("code", code),
        new KeyValuePair<string, string>("client_id", "581305694892-cqb0t3lrf76n6pn2kcsl5276lcrubaqi.apps.googleusercontent.com"),
        new KeyValuePair<string, string>("client_secret", "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"),
        new KeyValuePair<string, string>("redirect_uri", redirectUri),
        new KeyValuePair<string, string>("grant_type", "authorization_code")
    });

            var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var responseJson = JObject.Parse(responseString);
                return responseJson["access_token"].ToString();
            }

            return null;
        }

        private async Task<string> FetchEmailFromGoogle(string accessToken)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo?alt=json");

            if (response.IsSuccessStatusCode)
            {
                var userInformation = JObject.Parse(await response.Content.ReadAsStringAsync());
                return userInformation["email"].ToString();
            }

            return null;
        }



        public bool IsAdmin
        {
            get => _isAdmin;
            set => SetProperty(ref _isAdmin, value);
        }

        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private async Task PerformLogin()
        {
            var role = await DatabaseHelper.GetUserRoleAsync(UserName);

            if (role != null)
            {
                var appShell = (AppShell)App.Current.MainPage;

                if (role == "Admin")
                {
                    appShell.FindByName<FlyoutItem>("AdminPanelFlyoutItem").IsVisible = true;
                    appShell.FindByName<FlyoutItem>("FitnessPanelFlyoutItem").IsVisible = true;
                }
                else
                {
                    appShell.FindByName<FlyoutItem>("AdminPanelFlyoutItem").IsVisible = false;
                    appShell.FindByName<FlyoutItem>("FitnessPanelFlyoutItem").IsVisible = false;
                }

                appShell.SetFlyoutVisibility(true);
                await Shell.Current.GoToAsync("///dashboard");
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Login Failure", "Invalid username or password.", "OK");
            }
        }

        public static int FindFreePort()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private async Task NavigateToSignUp()
        {
            await Shell.Current.GoToAsync("///signup");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }
    }


}
