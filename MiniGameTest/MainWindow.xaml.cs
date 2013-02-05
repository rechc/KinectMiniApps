using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MiniGameTest
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private List<GridObjects> gridObjects= new List<GridObjects>();

        private FallWorker fallWorker;

        private Image player;

        private String path;

        private const String winter = "Bilder/Winter";
        private const String summer = "Bilder/Summer";

        /**
         * Konstruktor
         */
        public MainWindow()
        {
            InitializeComponent();

            this.KeyDown += new KeyEventHandler(KeyDownHandler);

            GameStart(1);
        }

        /**
         * Key-Handler zum Bewegen des Players
         */
        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                if (Grid.GetColumn(this.player) != 3)
                {
                    Grid.SetColumn(this.player, Grid.GetColumn(this.player) + 1);
                }
            }
            else if (e.Key == Key.Left)
            {
                if (Grid.GetColumn(this.player) != 1)
                {
                    Grid.SetColumn(this.player, Grid.GetColumn(this.player) - 1);
                }
            }
        }
        
        /**
         * Startet das Spiel
         */
        private void GameStart(int mode)
        {
            switch (mode)
            {
                case 0:
                    path = winter;
                    break;
                case 1:
                    path = summer;
                    break;
            }
            SetBackground();
            AddPlayer();
            fallWorker = new FallWorker(false);
            fallWorker.eventFallen += new MiniGameTest.FallWorker.FallWorkerEventHandler(FallHandler);
            new Thread(fallWorker.InvokeFalling).Start();
        }

        /**
         * Stopt das Spiel und entfernt die Objekte
         */
        private void GameStop()
        {
            this.Status.Visibility = Visibility.Visible;
            RemoveAllObjects();
            RemovePlayer();
        }

        /**
         * Handler der die Objekte nach unten bewegt, neue Objekte erzeugt und auf Spielende prüft
         */
        public void FallHandler(object sender, EventArgs ea)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                // GameOver überprüfen
                CheckGameOver();
                // Objekte nach unten setzen
                if (gridObjects.Count() > 0 && !fallWorker.getGameOver())
                {
                    ShiftObjects();
                }
                // GameOver überprüfen
                CheckGameOver();
                // Neue Objekte generieren
                if (!fallWorker.getGameOver())
                {
                    if (gridObjects.Count() < 3)
                    {
                        AddObjects();
                    }
                }
                else
                {
                    GameStop();
                }
            }));
        }

        /**
         * Schiebt die Objekte um eins nach unten im Grid
         */
        private void ShiftObjects()
        {
            List<GridObjects> tempGridObjects = new List<GridObjects>();
            foreach (GridObjects go in gridObjects)
            {
                if (go.row == 4)
                {
                    MiniGameGrid.Children.Remove(go.image);
                }
                else
                {
                    go.row++;
                    Grid.SetRow(go.image, go.row);
                    tempGridObjects.Add(go);
                }
            }
            gridObjects = tempGridObjects;
        }

        /**
         * Überprüft ob der Player und ein Objekt an gleicher Stelle liegt
         */
        private void CheckGameOver()
        {
            foreach (GridObjects go in gridObjects)
            {
                if (go.row == 4 && go.column == Grid.GetColumn(this.player))
                {
                    fallWorker.setGameOver(true);
                    break;
                }
            }
        }

        /**
         * Fügt ein Objekt dem Grid hinzu
         */
        private void AddObjects()
        {
            Image i = new Image();
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path + "/" + new Random().Next(1,3) + ".png", UriKind.Relative);
            bi.EndInit();
            i.Source = bi;
            MiniGameGrid.Children.Add(i);
            int column = new Random().Next(1, 4);
            Grid.SetColumn(i, column);
            Grid.SetRow(i, 1);
            gridObjects.Add(new GridObjects(i, column, 1));
        }

        /**
         * Löscht alle Objekte vom Grid, außer den Player
         */
        private void RemoveAllObjects()
        {
            foreach (GridObjects go in gridObjects)
            {
                MiniGameGrid.Children.Remove(go.image);
            }
            gridObjects.Clear();
        }

        /**
         * Fügt den Player dem Grid hinzu
         */
        private void AddPlayer()
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path + "/" + "player.png", UriKind.Relative);
            bi.EndInit();
            this.player = new Image();
            this.player.Source = bi;
            MiniGameGrid.Children.Add(this.player);
            Grid.SetColumn(this.player, 2);
            Grid.SetRow(this.player, 4);
        }

        /**
         * Löscht den Player vom Grid
         */
        private void RemovePlayer()
        {
            MiniGameGrid.Children.Remove(this.player);
        }

        /**
         * Setzt das Backgroundbild abhängig vom Modus
         */
        private void SetBackground()
        {
            String p = System.IO.Directory.GetCurrentDirectory();
            p = p.Replace("\\bin\\Debug", "");

            ImageBrush img = new ImageBrush();
            img.ImageSource = (ImageSource)new ImageSourceConverter().ConvertFromString(p + "\\" + path + "\\" + "background.jpg");
            MiniGameGrid.Background = img;
        }

        /**
         * Click Handler
         */
        private void Status_Click_1(object sender, RoutedEventArgs e)
        {
            GameStart(0);
            this.Status.Visibility = Visibility.Hidden;
        }
    }
}
