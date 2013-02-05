using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiniGameTest
{
    class FallWorker
    {

        public delegate void FallWorkerEventHandler(object sender, EventArgs e);
        public event FallWorkerEventHandler eventFallen;

        private bool gameOver;

        public FallWorker(bool gameOver)
        {
            this.gameOver = gameOver;
        }

        public void InvokeFalling()
        {
            while (!gameOver)
            {
                Thread.Sleep(1000);
                eventFallen(this, null);
            }
        }

        public bool getGameOver()
        {
            return this.gameOver;
        }

        public void setGameOver(bool gameOver)
        {
            this.gameOver = gameOver;
        }
    }
}
