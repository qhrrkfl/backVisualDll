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

    struct BaselineScale
    {
        // 그려질 스크린 스페이스 좌표
        public int camSpaceX, camSpaceY;
        public Size sz;
        // 표현하는 수학적 좌표
        public int scaleX, scaleY;
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
        public int hWid;
        public int hHei;

        /// <summary>
        /// 수학 좌표계의 extend임 world 상 cam의 위치
        /// </summary>
        /// <returns></returns>
        public Point[] getCamAreaVertexClockWise()
        {
            Point[] vertex = new Point[4];
            vertex[0] = new Point(center.X - hWid, center.Y + hHei);
            vertex[1] = new Point(center.X + hWid, center.Y + hHei);
            vertex[2] = new Point(center.X + hWid, center.Y - hHei);
            vertex[3] = new Point(center.X - hWid, center.Y - hHei);
            return vertex;
        }

        /// <summary>
        /// 수학좌표계(world 상 cam의 위치) 의 min max
        /// </summary>
        /// <returns></returns>
        public MinMaxInfo GetMinMaxInfo()
        {
            return UTIL.getMinMAx(getCamAreaVertexClockWise());
        }

        public bool PtinCamspace(int x , int y)
        {
            if(x >= -1*sz.Width/2 && x<= sz.Width/2 && y<= sz.Height/2 &&y >= -1*sz.Height/2)
            {
                return true;
            }
            else
            {
                return false;
            }
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
        public Cartisan(Size wndSize)
        {
            center = new Point(0, 0);
            world = new Point(0, 0);
            sz = wndSize;
            ExDots = 1;
            //clockwise

        }

        public override void draw(Graphics g, Extent cam)
        {

            List<Point> vertice = new List<Point>();
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

            // base라인 구축
            // 원점에서 camspace의 마지막 구간으로 뻗도록 설정.
            if(cam.PtinCamspace(X, Y))
            {
                vertice.Add(new Point(-cam.hWid , Y));
                vertice.Add(new Point(cam.hWid, Y));

                vertice.Add(new Point(X, -cam.hHei ));
                vertice.Add(new Point(X, cam.hHei ));
            }
            

            // screen space go
            for(int i=0;i<vertice.Count; i++)
            {
                int x = vertice[i].X;
                int y = vertice[i].Y;

                x += cam.hWid;
                y -= cam.hHei;

                y *= -1;

                vertice[i] = new Point(x, y);
            }



            for(int i=0;i<vertice.Count; i += 2)
            {
                g.DrawLine(Pens.Black, vertice[i], vertice[i + 1]);
            }


            
        }


    }


    class UTIL
    {
        public static int CentimeterToPixel(double Centimeter, Graphics g)
        {
            double pixel = -1;
            pixel = Centimeter * g.DpiY / 2.54d;
            return (int)pixel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="squareVertex"></param>
        /// <returns></returns>
        public static MinMaxInfo getMinMAx(Point[] squareVertex)
        {
            MinMaxInfo info;
            info.MinX = squareVertex[0].X;
            info.MaxX = squareVertex[2].X;
            info.MinY = squareVertex[2].Y;
            info.MaxY = squareVertex[0].Y;
            return info;

        }


        public static Point[] getAreaVertexClockWise(Point center, int hWid, int hHei)
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
        public static Point spToCartisan(Point scPt, Extent cam)
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


