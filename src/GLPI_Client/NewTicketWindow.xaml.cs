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

    public class ItilCategoryItem
    {
        public int id { get; set; }
        public string text { get; set; }
    }

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
    }
}
