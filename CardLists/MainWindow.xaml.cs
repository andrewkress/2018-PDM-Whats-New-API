using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CardLists {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            CardListViewModel clvm = new CardListViewModel();
            clvm.selectedVault = clvm.vaultNames.FirstOrDefault();
            this.DataContext = clvm;
        }
    }
}
