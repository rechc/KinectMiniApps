using Database;

namespace HtwKinect
{
    interface ISwitchableUserControl
    {
        /**
         * stops showing this UserControl in GUI
         * @returns the lastTravel
         */
        TravelOffer StopDisplay();

        /**
         * starts showing this UserControl with specified Travel
         */
        void StartDisplay(TravelOffer lastTravel);

    }
}
