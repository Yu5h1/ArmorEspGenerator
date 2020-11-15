using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Yu5h1Tools.WPFExtension;

namespace DefaultEquipmentsValueConfiguration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var panel = ControlUtil.CreateMixControls("A,B,C : ", new ComboBox() { Height = 30,Width = 200 });
            panel.VerticalAlignment = VerticalAlignment.Top;
            panel.SetPosition(10, 10);
            grid.Children.Add(panel);
            ControlUtil.CreateMixControls("Rating : ", new TextBox() { Height = 30, Width = 200 });
        }
    }
}
