using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiniGame
{
	/**
	 * Klasse die die Spielsteine des MiniGame fallen lässt
	 */
    class FallWorker
    {

        public delegate void FallWorkerEventHandler(object sender, EventArgs e);
        public event FallWorkerEventHandler eventFallen;

        public bool GameOver { get; set; }

		/**
		 * Konstruktor
		 */ 
        public FallWorker(bool gameOver)
        {
            this.GameOver = gameOver;
        }

		/**
		 * Fall-Thread
		 */
        public void InvokeFalling()
        {
            while (!this.GameOver)
            {
                Thread.Sleep(1200);
                eventFallen(this, null);
            }
        }
    }
}
