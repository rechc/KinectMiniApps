using System;
using System.Windows.Controls;

namespace HtwKinect.StateViews
{
    /// <summary>
    /// Interaktionslogik für SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : UserControl, ISwitchableUserControl
    {
        public SplashScreen()
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
