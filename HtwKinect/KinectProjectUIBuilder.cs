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
                do
                {
                    tmp.Above = first;
                    tmp = tmp.Right;
                } while (tmp != veryFirst);

                tmp = first;
                do
                {
                    tmp.Below = veryFirst;
                    tmp = tmp.Right;
                } while (tmp != first);

                tmp = _firstNodeOfLastRow;
                do
                {
                    tmp.Below = first;
                    tmp = tmp.Right;
                } while (tmp != _firstNodeOfLastRow);

                tmp = first;
                do
                {
                    tmp.Above = _firstNodeOfLastRow;
                    tmp = tmp.Right;
                } while (tmp != first);
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
