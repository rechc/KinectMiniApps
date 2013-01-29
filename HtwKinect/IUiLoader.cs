using System.Collections.Generic;
using System.Windows;
using LoopList;

namespace HtwKinect
{
    interface IUiLoader
    {
        void LoadElementsIntoList(KinectProjectUiBuilder kinectProjectUiBuilder);
    }
}
