using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Reactive.Disposables;
using ReactiveUI;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rack300SimulationUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ReactiveWindow<Rack300SimulationViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new Rack300SimulationViewModel();
            DataContext = ViewModel;
            this.WhenActivated(disposables => { });
        }
    }
}
