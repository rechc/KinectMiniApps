using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtwKinect
{
    interface SwitchableUserControl
    {
        /**
         * stops showing this UserControl in GUI
         * @returns the lastTravel
         */
        TravelOffer stopDisplay();

        /**
         * starts showing this UserControl with specified Travel
         */
        void startDisplay(TravelOffer lastTravel);

    }
}
