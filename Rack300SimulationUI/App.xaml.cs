using Splat;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ReactiveUI;
using System.Windows;

namespace Rack300SimulationUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TCPService _tcpService;
        public App()
        {

        }
            //Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Instantiate ViewModel
            var viewModel = new Rack300SimulationViewModel();

            // Instantiate and start TCP Service
            _tcpService = new TCPService(viewModel); // Pass ViewModel or an interface (recommended)
            _ = _tcpService.StartAsync(8080); // Start TCP server in background (non-blocking)

            // Create Main Window and assign DataContext
            var mainWindow = new MainWindow
            {
                DataContext = viewModel
            };

            mainWindow.Show();
        }
    }
}
