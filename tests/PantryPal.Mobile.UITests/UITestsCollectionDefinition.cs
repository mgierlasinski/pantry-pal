using Xunit;

namespace PantryPal.Mobile.UITests;

// Add a CollectionDefinition together with a ICollectionFixture
// to ensure that the setup only runs once
// xUnit does not have a built-in concept of a fixture that only runs once for the whole test set.
[CollectionDefinition("UITests")]
public sealed class UITestsCollectionDefinition : ICollectionFixture<AppiumSetup>
{

}