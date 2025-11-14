using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Microsoft.Maui.Controls;

namespace PantryPal.Mobile
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
        Exported = true)]
    [IntentFilter(
        new[] { Android.Content.Intent.ActionView },
        Categories = new[] { Android.Content.Intent.CategoryDefault, Android.Content.Intent.CategoryBrowsable },
        DataSchemes = new[] { "pantrypal" })]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            // Handle deep links when app is already running
            if (intent?.Action == Intent.ActionView && intent.Data != null)
            {
                var uri = new Uri(intent.Data.ToString());
                if (uri.Scheme == "pantrypal")
                {
                    // Create and fire the app link request
                    var app = (App)Microsoft.Maui.Controls.Application.Current!;
                    app.HandleDeepLink(uri);
                }
            }
        }
    }
}
