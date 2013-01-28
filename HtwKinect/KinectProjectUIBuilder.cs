using System;
using System.Collections.Generic;
using System.Windows;
using LoopList;

namespace HtwKinect
{
    /*Diese Klasse ordnet die UIElemente in der LoopList Projektspezifisch an. Sie dient hauptsächlich dem vereinfachten Zugriff auf die LoopList.*/
    class KinectProjectUiBuilder
    {
        private readonly LoopList.LoopList _loopList;
        private readonly TextLoopList _textLoopList;
        private Node _firstNodeOfLastRow;
        private readonly Dictionary<string, Node> _rows = new Dictionary<string, Node>(); 

        public KinectProjectUiBuilder(LoopList.LoopList loopList, TextLoopList textLoopList)
        {
            _loopList = loopList;
            _textLoopList = textLoopList;
        }

        public void AddRow(String rowName, List<FrameworkElement> row)
        {
            _textLoopList.Add(rowName);
            Node anchor = null;
            Node first = null;
            foreach (var frameworkElement in row)
            {
                anchor = _loopList.AddToRight(anchor, frameworkElement);
                if (first == null)
                    first = anchor;
            }

            if (first == null)
                throw new Exception("Given row has no elements to add");
            _rows.Add(rowName, first);
            if (_firstNodeOfLastRow != null)
            {
                    
                Node veryFirst = _firstNodeOfLastRow.Below;
                Node tmp = veryFirst;
                while (!tmp.MarkedRight)
                {
                    tmp.Above = first;
                    tmp = tmp.Right;
                }
                tmp.Above = first;
                tmp = first;
                while (!tmp.MarkedRight)
                {
                    tmp.Below = veryFirst;
                    tmp = tmp.Right;
                }
                tmp.Below = veryFirst;
                tmp = _firstNodeOfLastRow;
                while (!tmp.MarkedRight)
                {
                    tmp.Below = first;
                    tmp.MarkedBelow = false;
                    tmp = tmp.Right;
                }
                tmp.Below = first;
                tmp.MarkedBelow = false;
                tmp = first;
                while (!tmp.MarkedRight)
                {
                    tmp.Above = _firstNodeOfLastRow;
                    tmp.MarkedAbove = false;
                    tmp = tmp.Right;
                }
                tmp.Above = _firstNodeOfLastRow;
                tmp.MarkedAbove = false;
            }
            _firstNodeOfLastRow = first;
        }

        /*Beziehen des Anfangsknotens einer Row. Die Row wird über GetRight / GetLeft navigiert. Die Grenzen sind für IsMarkedLeft/IsMarkedRight == true*/
        public Node GetRowByRowName(string rowName)
        {
            Node node;
            _rows.TryGetValue(rowName, out node);
            return node;
        }
    }
}
