using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using LoopList;

namespace LoopListTest
{
    class KinectProjectUiBuilder
    {
        private readonly LoopList.LoopList _loopList;
        private readonly TextLoopList _textLoopList;
        private Node _firstNodeOfLastRow;

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
            if (first != null)
            {
                if (_firstNodeOfLastRow != null)
                {
                    
                    Node veryFirst = _firstNodeOfLastRow.GetBelow();
                    Node tmp = veryFirst;
                    bool stop = false;
                    while (true)
                    {
                        tmp.SetAbove(first);
                        tmp = tmp.GetRight();
                        if (stop)
                            break;
                        if (tmp.IsMarkedRight())
                        {
                            stop = true;
                        }
                    }
                    stop = false;
                    tmp = first;
                    while (true)
                    {
                        tmp.SetBelow(veryFirst);
                        tmp = tmp.GetRight();
                        if (stop)
                            break;
                        if (tmp.IsMarkedRight())
                        {
                            stop = true;
                        }
                    }
                    stop = false;
                    tmp = _firstNodeOfLastRow;
                    while (true)
                    {
                        tmp.SetBelow(first);
                        tmp.UnmarkBelow();
                        tmp = tmp.GetRight();
                        if (stop)
                            break;
                        if (tmp.IsMarkedRight())
                        {
                            stop = true;
                        }
                    }
                    stop = false;
                    tmp = first;
                    while (true)
                    {
                        tmp.SetAbove(_firstNodeOfLastRow);
                        tmp.UnmarkAbove();
                        tmp = tmp.GetRight();
                        if (stop)
                            break;
                        if (tmp.IsMarkedRight())
                        {
                            stop = true;
                        }
                    }
                }
                _firstNodeOfLastRow = first;
            }
        }
    }
}
