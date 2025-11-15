using OpenQA.Selenium.Appium;

namespace PantryPal.Mobile.UITests.Pages.Components;

public class MaterialTextField
{
    private readonly AppiumElement _parent;

    public AppiumElement EditText => _parent.FindElement(MobileBy.ClassName("android.widget.EditText"));
    public AppiumElement ValidationError => _parent.FindElement(MobileBy.ClassName("android.widget.TextView"));

    public MaterialTextField(AppiumElement parent)
    {
        _parent = parent;
    }
}
