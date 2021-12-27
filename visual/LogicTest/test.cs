using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
namespace visual.LogicTest
{
    struct MinMaxInfo
    {
        public int MaxX;
        public int MinX;
        public int MaxY;
        public int MinY;
    }

    class Extent
    {
        public Extent(Size sz, Point pt)
        {
            this.sz = sz;
            this.center = pt;

            hWid = sz.Width / 2;
            hHei = sz.Height / 2;

         

        }
        // clockwise
        
        public Point center;
        public Size sz;
        int hWid;
        int hHei;


        public Point[] getCamAreaVertexClockWise()
        {
            Point[] vertex = new Point[4];
            vertex[0] = new Point(center.X - hWid, center.Y + hHei);
            vertex[1] = new Point(center.X + hWid, center.Y + hHei);
            vertex[2] = new Point(center.X + hWid, center.Y - hHei);
            vertex[3] = new Point(center.X - hWid, center.Y - hHei);
            return vertex;
        }


        public MinMaxInfo GetMinMaxInfo()
        {
           return UTIL.getMinMAx(getCamAreaVertexClockWise());
        }
    }

    abstract class Object
    {
        public Point world;
        public Point center;
        public Size sz;
        public abstract void draw(Graphics g, Extent cam);
    }

    class Cartisan : Object
    {
        /// <summary>
        /// 좌표 간격
        /// </summary>
        public double ExDots;
        public Cartisan( Size wndSize )
        {
            center = new Point(0, 0);
            world = new Point(0, 0);
            sz = wndSize;
            ExDots = 1;
            //clockwise
            
        }

        public override void draw(Graphics g, Extent cam)
        {
            // local to world
            int X, Y;
            X = center.X + world.X;
            Y = center.X + world.Y;

            // world to camspace
            X -= cam.center.X;
            Y -= cam.center.Y;



            // save this for math
            int camSpX = X;
            int camSpY = Y;

            // half cam size
            int hWid = cam.sz.Width / 2;
            int hHei = cam.sz.Height / 2;

            MinMaxInfo camMinMax = cam.GetMinMaxInfo();
            MinMaxInfo blMinMax = UTIL.getMinMAx(UTIL.getAreaVertexClockWise(new Point(camSpX, camSpY), hWid, hHei));


            int extendMinX = 0;
            int extendMaxX = 0;
            int extendMinY = 0;
            int extendMaxY = 0;

            // keep in mind that this is just scaler value
            if (camMinMax.MinX < blMinMax.MinX)
            {
                extendMinX = blMinMax.MinX - camMinMax.MinX;
            }

            if (camMinMax.MaxX > blMinMax.MaxX)
            {
                extendMaxX = camMinMax.MaxX - blMinMax.MaxX;
            }

            if (camMinMax.MinY < blMinMax.MinY)
            {
                extendMinY = blMinMax.MinY - camMinMax.MinY;
            }

            if (camMinMax.MaxY > blMinMax.MaxY)
            {
                extendMaxY = camMinMax.MaxY - blMinMax.MaxY;
            }


            //Debug.WriteLine(string.Format("exMaxX : {0} exMinx : {1} exMinY : {2} exMaxY {3}", extendMaxX , extendMinX , extendMinY , extendMaxY));




            
            // Caartian coordinates to screenSapce
            X += hWid;
            // screen and cartisan has opposite Y axis
            Y -= hHei;
            // revert it
            Y *= -1;

            //now it's screen space and extend baseline
            
            Point[] vertex = new Point[4];
            vertex[0] = new Point(X - hWid - extendMinX, Y );
            vertex[1] = new Point(X + hWid + extendMaxX, Y );
            vertex[2] = new Point(X , Y + hHei + extendMinY);
            vertex[3] = new Point(X , Y - hHei - extendMaxY);


            // draw dots...

            Debug.WriteLine(string.Format(" screen space X : {0} , Y : {1}", X, Y));
            





            //draw
            g.DrawLine(Pens.Black, vertex[0], vertex[1]);
            g.DrawLine(Pens.Black, vertex[2], vertex[3]);
            int dotInterval = UTIL.CentimeterToPixel(1, g);

        }

       
    }


    class UTIL
    {
        public static int CentimeterToPixel(double Centimeter , Graphics g)
        {
            double pixel = -1;
            pixel = Centimeter * g.DpiY / 2.54d;
            return (int)pixel;
        }

       public static MinMaxInfo getMinMAx(Point[] squareVertex  )
        {
            MinMaxInfo info;
            info.MinX = squareVertex[0].X;
            info.MaxX = squareVertex[2].X;
            info.MinY = squareVertex[2].Y;
            info.MaxY = squareVertex[0].Y;
            return info;

        }


        public static Point[] getAreaVertexClockWise(Point center , int hWid, int hHei)
        {
            Point[] vertex = new Point[4];
            vertex[0] = new Point(center.X - hWid, center.Y + hHei);
            vertex[1] = new Point(center.X + hWid, center.Y + hHei);
            vertex[2] = new Point(center.X + hWid, center.Y - hHei);
            vertex[3] = new Point(center.X - hWid, center.Y - hHei);
            return vertex;
        }

        /// <summary>
        /// 좌표계 변환 screen to baseline world coordinates
        /// </summary>
        public static Point spToCartisan(Point scPt , Extent cam)
        {
            int bX = scPt.X;
            int bY = scPt.Y;
            int hHei = cam.sz.Height / 2;
            int hWid = cam.sz.Width / 2;
            // test revert screen to cartisan
            // revert Y axis to cartisan
            bY *= -1;
            // invert screen space transform
            bY += hHei;
            bX -= hWid;

            //invert camspace to world
            // world to camspace
            bX += cam.center.X;
            bY += cam.center.Y;

            return new Point(bX, bY);
        }


    }

}


