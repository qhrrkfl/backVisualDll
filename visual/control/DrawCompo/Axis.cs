using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using visual.control.DrawCompo.Interface;
namespace visual.control.DrawCompo
{
    class Axis : IDraw
    {
        bool _isVertical = false;
        Point _loc;

        public Axis(Point p) 
        {
            _loc = p;

        }

        public void Draw(Graphics g)
        {
            
        }

    }
}
