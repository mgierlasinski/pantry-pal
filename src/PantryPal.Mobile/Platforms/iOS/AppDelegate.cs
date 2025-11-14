using Foundation;
using UIKit;
using Microsoft.Maui.Controls;

namespace PantryPal.Mobile
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
        {
            // Handle deep links when app is already running on iOS
            if (url.Scheme == "pantrypal")
            {
                var uri = new Uri(url.AbsoluteString);
                var app = (App)Microsoft.Maui.Controls.Application.Current!;
                app.HandleDeepLink(uri);
                return true;
            }

            return base.OpenUrl(application, url, options);
        }
    }
}
