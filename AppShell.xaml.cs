using System.Diagnostics;

namespace DSR
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        public void SetFlyoutItemVisibility(string role)
        {
            var pageItem = this.FindByName<FlyoutItem>("PageFlyoutItem");
            var dashboardItem = this.FindByName<FlyoutItem>("DashboardFlyoutItem");
            var adminPanelItem = this.FindByName<FlyoutItem>("AdminPanelFlyoutItem");
            var fitnessPanelItem = this.FindByName<FlyoutItem>("FitnessPanelFlyoutItem");


            if (role == "Admin")
            {
                adminPanelItem.IsVisible = true;
                fitnessPanelItem.IsVisible = true;
                dashboardItem.IsVisible = true;
            }
            if (role == "User")
            {

                Debug.WriteLine("Non-Admin user: All items hidden.");
                dashboardItem.IsVisible = false;
                adminPanelItem.IsVisible = false;
                fitnessPanelItem.IsVisible = false;

            }
        }
        public void SetFlyoutVisibility(bool isVisible)
        {
            this.FlyoutBehavior = isVisible ? FlyoutBehavior.Flyout : FlyoutBehavior.Disabled;
        }
    }
}
