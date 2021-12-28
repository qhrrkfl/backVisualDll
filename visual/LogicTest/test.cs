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
        int hWid;
        int hHei;

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

            int screenextendMinX = 0;
            int screenextendMaxX = 0;
            int screenextendMinY = 0;
            int screenextendMaxY = 0;

            //keep in mind that this is just scaler value
            // 원본
            if (camMinMax.MinX < blMinMax.MinX)
            {
                screenextendMinX = blMinMax.MinX - camMinMax.MinX;
            }

            if (camMinMax.MaxX > blMinMax.MaxX)
            {
                screenextendMaxX = camMinMax.MaxX - blMinMax.MaxX;
            }

            if (camMinMax.MinY < blMinMax.MinY)
            {
                screenextendMinY = blMinMax.MinY - camMinMax.MinY;
            }

            if (camMinMax.MaxY > blMinMax.MaxY)
            {
                screenextendMaxY = camMinMax.MaxY - blMinMax.MaxY;
            }

         
            extendMinX = blMinMax.MinX - camMinMax.MinX;
            extendMaxX = camMinMax.MaxX - blMinMax.MaxX;
            extendMinY = blMinMax.MinY - camMinMax.MinY;
            extendMaxY = camMinMax.MaxY - blMinMax.MaxY;


            // 좌표의 확장기준이 camspace에서 계산되니
            // dot의 위치도 camspace에서 계산하며
            // 확장된 부분의 수학 좌표도 계산하여 붙여준다.
            // camspace는 cam이 0,0 이며 크기는 cam크기이다.
            int dotInterval = UTIL.CentimeterToPixel(1, g);
            List<BaselineScale> xDots = new List<BaselineScale>();
            
            float stScaleX;
            float endScaleX;

            //Debug.WriteLine(string.Format("camMinX : {0} camMaxX : {1}", camMinMax.MinX, camMinMax.MaxX));
            //Debug.WriteLine(string.Format("spX : {0} blminx {1}   blmaxx{2}", camSpX, blMinMax.MinX, blMinMax.MaxX));

            Debug.WriteLine(string.Format("camX {0}  camY {1}", camSpX, camSpY));

            Point[] exBL = new Point[4];
            
            // 20211229 오늘 삽질 결과
            // 1. camspace에서 0,0 에서 cam 안에 들어오면 스크린에 나와야하는게 정상이다
            // 2. 지금 원점을 camspace에 두면 0,0이 아닌 이동한다.
            // 3. 원점 오브젝트를 캠스페이스에서 캠에 맞도록 변형시키는 작업이 필요하다
            // 4. 지금은 계산이 캠의 월드 좌표와 물건의 camspace좌표를 이용해서 구함
            // 5. 그러니까 지금 계산이 틀어져서 원래 안맞느건데 재수좋게 맞아서 잘 보였던거임

            // 다시.
            // 1. 지금 늘리는 로직은 폐기하고
            // 2. 캠스페이스에 맞도록 원점 object를 조절하도록 한다.
            // 3. 이건 원점이 캠스페이스의 어느 방향으로 뻗어나갔느냐에 따라 다름.(분기)
            // 4. 캠스페이스에서 조절된 물체는 일괄적으로 screenspace로 변환하면됨.

            // 5. 변형 로직과 닷 찍는것과의 상관관계가 있나?
            // 
            // x------x
            // 음수 확장
            //exBL[0] = new Point(camSpX - hWid - extendMinX, camSpY);
            BaselineScale startX;
            startX.scaleX = (int)((camSpX - hWid - extendMinX) / dotInterval);
            startX.scaleY = 0;
            startX.camSpaceX = startX.scaleX * dotInterval;
            startX.camSpaceY = camSpY;

            Debug.WriteLine(string.Format("stScale : {0}", startX.camSpaceX));

            // 양수 확장
            //exBL[1] = new Point(camSpX + hWid + extendMaxX, camSpY);


            BaselineScale endX;
            endX.scaleX = (int)((camSpX + hWid + extendMaxX) / dotInterval);
            endX.scaleY = 0;
            endX.camSpaceX = endX.scaleX * dotInterval;
            endX.camSpaceY = camSpY;

            Debug.WriteLine(string.Format("endScaleX : {0}", endX.camSpaceX));

            /* y
               |
               Y */
            // 양수확장
            //exBL[2] = new Point(camSpX, camSpY + hHei + extendMinY);
            BaselineScale endY;
            endY.scaleX = 0;
            endY.scaleY = ((camSpY + hHei + extendMinY)/ dotInterval);
            endY.camSpaceX = camSpX;
            endY.camSpaceY = endY.scaleY * dotInterval; 
            // 음수 확장
            //exBL[3] = new Point(camSpX, camSpY - hHei - extendMaxY);
            BaselineScale stY;
            stY.scaleX = 0;
            stY.scaleY = ((camSpY - hHei - extendMaxY) / dotInterval);
            stY.camSpaceX = camSpX;
            stY.camSpaceY = stY.scaleY * dotInterval;

            //Debug.WriteLine(string.Format("STScaleY : {0}", stY.scaleY));
            //Debug.WriteLine(string.Format("endScaleY : {0}", endY.scaleY));


            //Debug.WriteLine(string.Format("exMaxX : {0} exMinx : {1} exMinY : {2} exMaxY {3}", extendMaxX , extendMinX , extendMinY , extendMaxY));


            //int xdotCnt = cam.sz.Width / dotInterval;
            //int ydotCnt = cam.sz.Height / dotInterval;


            
            //Point[] vertex = new Point[5];

            //// origin
            //vertex[4].X = camSpX;
            //vertex[4].Y = camSpY;

            ////clock wise...
            //vertex[0].X = startX.camSpaceX;
            //vertex[0].Y = startX.camSpaceY;

            //vertex[1].X = endX.camSpaceX;
            //vertex[1].Y = endX.camSpaceY;

            //vertex[2].X = stY.camSpaceX;
            //vertex[2].Y = stY.camSpaceY;

            //vertex[3].X = endY.camSpaceX;
            //vertex[3].Y = endY.camSpaceY;

            //Debug.WriteLine(vertex[0].X);

            //for(int i=0; i<vertex.Length; i++)
            //{
            //    vertex[i].X += hWid;
            //    vertex[i].Y -= hHei;
            //    vertex[i].Y *= -1;
            //}

            //Debug.WriteLine(vertex[0].X);



            // Caartian coordinates to screenSapce
            X += hWid;
            // screen and cartisan has opposite Y axis
            Y -= hHei;
            // revert it
            Y *= -1;

            //now it's screen space and extend baseline

            Point[] vertex = new Point[4];
            //vertex[0] = new Point(X - hWid - screenextendMinX, Y);
            //vertex[1] = new Point(X + hWid + screenextendMaxX, Y);
            //// y is reverted
            //vertex[2] = new Point(X, Y + hHei + screenextendMinY);
            //vertex[3] = new Point(X, Y - hHei - screenextendMaxY);


            vertex[0] = new Point(X - hWid , Y);
            vertex[1] = new Point(X + hWid , Y);
            // y is reverted
            vertex[2] = new Point(X, Y + hHei );
            vertex[3] = new Point(X, Y - hHei );

            // draw dots...




            g.DrawLine(Pens.Black, vertex[0], vertex[1]);
            g.DrawLine(Pens.Black, vertex[2], vertex[3]);



            //draw
            //g.DrawLine(Pens.Black, vertex[0], vertex[4]);
            //g.DrawLine(Pens.Black, vertex[1], vertex[4]);
            //g.DrawLine(Pens.Black, vertex[2], vertex[4]);
            //g.DrawLine(Pens.Black, vertex[3], vertex[4]);
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


