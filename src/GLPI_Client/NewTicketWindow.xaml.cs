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

namespace GLPI_Client
{


    /// <summary>
    /// Interaction logic for NewTicketWindow.xaml
    /// </summary>
    public partial class NewTicketWindow : Window
    {
        public GLPI glpi;

        public NewTicketWindow(GLPI glpi)
        {
            InitializeComponent();
            this.glpi = glpi;


            // preenche a lista de categorias de chamado
            
            // Criar lista vazia de categorias de itil
            List<ItilCategoryItem> itilCategoryList = new List<ItilCategoryItem>();

            // Preencher a lista com o dicionário obtido do glpi
            foreach (var obj in glpi.getItilCategories())
            {
                itilCategoryList.Add(new ItilCategoryItem() {id=obj.Key, text=obj.Value });
            }

            // Atualiza o controle com as categorias com a lista populada anteriormente
            cmbItilCategories.ItemsSource = itilCategoryList;
            cmbItilCategories.DisplayMemberPath = "text";
            cmbItilCategories.SelectedValuePath = "id";

            
        }



        private void btnNewTicket_Click(object sender, RoutedEventArgs e)
        {
            // Button btnNewTicket = (Button) sender;
            TextBox txtTitle = (TextBox) this.FindName("txtTitle");
            RichTextBox txtDetails = (RichTextBox) this.FindName("txtDetails");
            string details = new TextRange(txtDetails.Document.ContentStart, txtDetails.Document.ContentEnd).Text;
            ComboBox itilCategories = (ComboBox)this.FindName("cmbItilCategories");
            // System.Windows.MessageBox.Show(itilCategories.ToString());
            int itilCategoriesId = int.Parse( itilCategories.SelectedValue.ToString() );
            int ticketId = this.glpi.createNewTicket(txtTitle.Text, details, itilCategoriesId);
            if (ticketId == 0)
            {
                System.Windows.MessageBox.Show("Erro ao cadastrar chamado!");
            }
            else
            {
                System.Windows.MessageBox.Show("Chamado registrado!\nNúmero: " + ticketId.ToString());
                this.Close();
            }
        }
    }


    public class ItilCategoryItem
    {
        public int id { get; set; }
        public string text { get; set; }
    }
}
