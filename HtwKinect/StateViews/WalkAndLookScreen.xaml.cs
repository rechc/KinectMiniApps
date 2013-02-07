using System;
using System.Windows.Controls;

namespace HtwKinect.StateViews
{
    /// <summary>
    /// Interaktionslogik für WalkAndLookScreen.xaml
    /// </summary>
    public partial class WalkAndLookScreen : UserControl, ISwitchableUserControl
    {
        public WalkAndLookScreen()
        {
            InitializeComponent();
        }

        public Database.TravelOffer StopDisplay()
        {
            throw new NotImplementedException();
        }

        public void StartDisplay(Database.TravelOffer lastTravel)
        {
            throw new NotImplementedException();
        }
    }
}
