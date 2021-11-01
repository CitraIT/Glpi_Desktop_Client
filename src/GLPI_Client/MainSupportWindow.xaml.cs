using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainSupportWindow.xaml
    /// </summary>
    public partial class MainSupportWindow : Window 
    {
        public GLPI glpi;
        public MainSupportWindow(GLPI glpi)
        {
            this.glpi = glpi;
            InitializeComponent();
        }

        private void btnNewTicket_Click(object sender, RoutedEventArgs e)
        {
            NewTicketWindow ticketWindow = new NewTicketWindow(glpi);
            ticketWindow.ShowDialog();
        }
    }
}
