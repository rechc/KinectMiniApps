using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace WpfApplication1
{

    class CircleList <E>
    {
        private List<E> list = new List<E>();
        public void add(E e)
        {
            list.Add(e);
        }

        public E getElementAt(int index)
        {
            return list[index];
        }
        public E getNext(int index)
        {
            
            if (index >= list.Count) return default(E);
            E e = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                list[i] = list[i + 1];
            }
            list[list.Count - 1] = e;
            return list[index];
        }

        public E getPrevious()
        {
            if (list.Count == 0) return default(E);
            E e = list[list.Count - 1];
            for (int i = list.Count - 1; i > 0; i--)
            {
                list[i] = list[i - 1];
            }
            list[0] = e;
            return list[0];
        }

        public int Count
        {
            get
            {
                return list.Count;
            }
        }
    }
}
