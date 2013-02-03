using Database.DAO;
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
using System.Windows.Shapes;

namespace InfoBanner
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        private TravelOfferDao _offerDao;

        public UserControl1()
        {
            InitializeComponent();
            _offerDao = new TravelOfferDao();
        }

        public void InvalidateVisual()
        {
            base.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            GetDbData(drawingContext);
        }

        private void GetDbData(DrawingContext drawingContext)
        {
            string categorie = _offerDao.SelectAllOffers().First().Country.CountryName;
            Categorie.Content = categorie;

        }
    }
}
