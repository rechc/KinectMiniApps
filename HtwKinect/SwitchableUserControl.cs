using Database;

namespace HtwKinect
{
    interface ISwitchableUserControl
    {
        /// <summary>
        /// Stops showing this UserControl in GUI.
        /// </summary>
        /// <returns>Returns the last travel.</returns>
        TravelOffer StopDisplay();

        /// <summary>
        /// Starts showing this UserControl with specified Travel.
        /// </summary>
        void StartDisplay(TravelOffer lastTravel);
    }
}
