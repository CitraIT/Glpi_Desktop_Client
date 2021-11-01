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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;


namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            txtLogin.Text = Environment.ExpandEnvironmentVariables("%username%");
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string url = @"https://suporte.citrait.com.br/";
            GLPI glpi = new GLPI(url, txtLogin.Text, txtPassword.Password);
            
            if (glpi.doLogin())
            {
                MainSupportWindow mainForm = new MainSupportWindow(glpi);
                mainForm.Show();
                this.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Usuário ou senha inválidos.");
            }
        }
    }
}
