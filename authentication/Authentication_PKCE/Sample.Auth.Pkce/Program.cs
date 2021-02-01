using Newtonsoft.Json;
using Sample.Auth.Pkce.Models;
using Sample.Auth.Pkce.Services;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Sample.Auth.Pkce
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var app = GetApp();
                var authService = new PkceAuthService();

                // Open Listener for Redirect
                var listener = BeginListening(app);

                // Getting first token 
                Console.WriteLine("Getting the initial token... ");
                var token = GoLogin(app, listener);

                // Call APIs
                var clientService = new ClientService(app.OpenApiBaseUrl, token.AccessToken, token.TokenType);

                // get Client details
                Console.WriteLine("API call: getting client details... ");
                clientService.WriteFile(clientService.GetClient());

                // get user details
                Console.WriteLine("API call: getting user details... ");
                clientService.WriteFile(clientService.GetUser());

                // get instruments
                Console.WriteLine("API call: list instruments... ");
                clientService.WriteFile(clientService.GetInstruments("DKK", "FxSpot"));

                // place an order
                Console.WriteLine("API call: place an order... ");
                Order placedOrder = new Order { 
                    Uic = 2, AccountKey = app.AccountKey, BuySell = "Buy", 
                    AssetType = "FxSpot", Amount = 100000, OrderPrice = 7, 
                    OrderType = "Limit", OrderRelation = "StandAlone", 
                    ManualOrder = true, 
                    OrderDuration = new classOrderDuration() { DurationType = "GoodTillCancel"} };
                clientService.WriteFile(clientService.PlaceOrder(placedOrder));

                // retrieve all orders
                Console.WriteLine("API call: retrieve all available orders from the main account... ");
                clientService.WriteFile(clientService.GetOrders());

                // Refresh token and call api
                Console.WriteLine("Refreshing token... ");
                var newToken = RefreshToken(app, token.RefreshToken, listener);
                Console.WriteLine("Token has been refreshed and the new value is: ");
                Console.WriteLine(JsonConvert.SerializeObject(new { Token = newToken }, Formatting.Indented));
                Console.WriteLine("Demo is complete.");
                Console.WriteLine("================================ ");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.Read();
            }
        }


        private static Token GoLogin(App app, HttpListener listener)
        {
            var authService = new PkceAuthService();
            var authUrl = authService.GetAuthenticationRequest(app);

            System.Diagnostics.Process.Start(authUrl);

            var authCode = GetAuthCode(app, listener);
            Console.WriteLine($"Auth code {authCode} received.");

            // Get Token
            return authService.GetToken(app, authCode);
        }


        private static Token RefreshToken(App app, string refreshToken, HttpListener listener)
        {
            var authService = new PkceAuthService();
            if (string.IsNullOrEmpty(refreshToken))
                throw new ArgumentException("Invalid refresh token");

            var token = authService.RefreshToken(app, refreshToken);

            return token;
        }

        private static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private static HttpListener BeginListening(App app)
        {
            HttpListener listener;
            try
            {
                var port = GetRandomUnusedPort();
                var uri = new Uri(app.RedirectUrls[0]);
                listener = new HttpListener();
                listener.Prefixes.Add($"{uri.Scheme}://{uri.Host}:{port}/");
                listener.Start();

                app.RedirectUrls[0] = uri.AbsoluteUri.Replace(uri.Host, uri.Host + ":" + port);
                return listener;
            }
            catch(Exception ex)
            {
                throw new Exception("Failed to start the listener for the redirect URL", ex);
            }
        }

        private static string GetAuthCode(App app, HttpListener listener)
        {
            // Listening
            HttpListenerContext httpContext = null;
            try
            {
                httpContext = listener.GetContext();
                var authCode = httpContext.Request.QueryString["code"];
                using (var writer = new StreamWriter(httpContext.Response.OutputStream))
                {
                    writer.WriteLine("AuthCode received by App. Please close the browser.");
                    writer.Close();
                }

                return authCode;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get the authCode from URL", ex);
            }
            finally
            {
                if (httpContext != null)
                    httpContext.Response.Close();
            }
        }

        private static App GetApp()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "App.json");
            var content = System.IO.File.ReadAllText(path);
            return JsonConvert.DeserializeObject<App>(content);
        }
    }
}
