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

namespace HtwKinect.StateViews
{
    /// <summary>
    /// Interaktionslogik für WalkAndLookScreen.xaml
    /// </summary>
    public partial class WalkAndLookScreen : UserControl, SwitchableUserControl
    {
        public WalkAndLookScreen()
        {
            InitializeComponent();
        }

        public Database.TravelOffer stopDisplay()
        {
            throw new NotImplementedException();
        }

        public void startDisplay(Database.TravelOffer lastTravel)
        {
            throw new NotImplementedException();
        }
    }
}
