using DSR;
using System.Diagnostics;


namespace DSR
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            try
            {
                MainPage = new AppShell(); 
                DatabaseHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
               
                Debug.WriteLine($"Error during app initialization: {ex.Message}");
            }
        }


    }
}