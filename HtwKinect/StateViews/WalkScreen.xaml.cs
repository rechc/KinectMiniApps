using System;
using System.Windows.Controls;

namespace HtwKinect.StateViews
{
    /// <summary>
    /// Interaktionslogik für WalkScreen.xaml
    /// </summary>
    public partial class WalkScreen : UserControl , ISwitchableUserControl
    {
        public WalkScreen()
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
