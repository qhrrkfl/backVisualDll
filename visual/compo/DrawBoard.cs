using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using visual.LogicTest;
using System.Diagnostics;

namespace visual.compo
{
    public partial class DrawBoard : UserControl
    {
        bool debug = true;
        bool bRbtnDown = false;
        Point ptOld;
        Extent cam;
        List<visual.LogicTest.Object> lstObject = new List<LogicTest.Object>();
        public DrawBoard()
        {
            InitializeComponent();
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            cam = new Extent(this.Size, new Point(0, 0));
            lstObject.Add(new Cartisan(this.Size));
            this.DoubleBuffered = true;
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            foreach (var ele in lstObject)
            {
                ele.draw(e.Graphics, cam);

            }

            if (debug)
            {
                e.Graphics.DrawString(string.Format("X: {0} , Y: {1}", cam.center.X, cam.center.Y)
                    , DefaultFont, Brushes.Black, new Point(0, 0));
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Right)
            {
                bRbtnDown = true;
                ptOld = e.Location;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Right)
            {
                bRbtnDown = false;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (bRbtnDown)
            {
                
                
                // this is screen space value
                int dX = e.X - ptOld.X;
                int dY = e.Y - ptOld.Y;
                // makeit cartisan
                dY*= -1;

                // 카메라 움직임을 반대로 하도록 변경. 헷갈리니까~ 사람들은 좌표평면을 움직인다는 생각을 하게끔..
                dX *= -1;
                dY *= -1;


                //Debug.WriteLine(string.Format("X : {0} , Y : {1}", dX, dY));


                cam.center.X += dX;
                cam.center.Y += dY;
                //Debug.WriteLine(string.Format("camPos X : {0} , Y : {1}", cam.center.X, cam.center.Y));

              

                ptOld = e.Location;
                this.Refresh();
            }

            //Point ptCartisan = UTIL.spToCartisan(e.Location, cam);
            //Debug.WriteLine(string.Format("converted X : {0} y: {1}", ptCartisan.X, ptCartisan.Y));
        }



    }
}
