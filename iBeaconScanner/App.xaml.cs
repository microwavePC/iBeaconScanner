using Prism.Unity;
using iBeaconScanner.Views;

namespace iBeaconScanner
{
	public partial class App : PrismApplication
	{
		public App(IPlatformInitializer initializer = null) : base(initializer) { }

		protected override void OnInitialized()
		{
			InitializeComponent();
			NavigationService.NavigateAsync("MainPage");
		}

		protected override void RegisterTypes()
		{
			Container.RegisterTypeForNavigation<MainPage>();
		}
	}
}

