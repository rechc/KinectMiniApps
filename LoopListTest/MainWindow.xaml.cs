using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

namespace LoopListTest
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point? oldMouseMovePoint;
        private bool doDrag;

        public MainWindow()
        {
            InitializeComponent();
            string[] paths = Directory.GetFiles(Environment.CurrentDirectory + @"\images");
            foreach (string path in paths)
            {
                myLoopList.add(path);
            }
        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                myLoopList.leftAnim();
            }
            if (e.Key == Key.Right)
            {
                myLoopList.rightAnim();
            }
        }

        private void myLoopList_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (doDrag)
            {
                Point currentPos = e.GetPosition(myLoopList);
                if (!oldMouseMovePoint.HasValue)
                {
                    oldMouseMovePoint = currentPos;
                }
                if (oldMouseMovePoint.HasValue && oldMouseMovePoint.Value.X == currentPos.X)
                {
                    return;
                }

                int xDistance = (int)(currentPos.X - oldMouseMovePoint.Value.X);
                bool mayDragOn = myLoopList.drag(xDistance);
                if (!mayDragOn)
                {
                    doDrag = false;
                }
                oldMouseMovePoint = currentPos;
            }
        }

        private void myLoopList_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            doDrag = false;
            oldMouseMovePoint = null;
        }

        private void myLoopList_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            doDrag = true;
        }

    }
}
