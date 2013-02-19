using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiniGame
{
    class FallWorker
    {

        public delegate void FallWorkerEventHandler(object sender, EventArgs e);
        public event FallWorkerEventHandler eventFallen;

        public bool GameOver { get; set; }

        public FallWorker(bool gameOver)
        {
            this.GameOver = gameOver;
        }

        public void InvokeFalling()
        {
            while (!this.GameOver)
            {
                Thread.Sleep(1000);
                eventFallen(this, null);
            }
        }
    }
}
