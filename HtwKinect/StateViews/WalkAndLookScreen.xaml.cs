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
            // TODO implement
            return null;
        }

        public void StartDisplay(Database.TravelOffer lastTravel)
        {
            // TODO implement
        }
    }
}
