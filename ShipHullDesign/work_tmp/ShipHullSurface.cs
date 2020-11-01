﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Controls;

namespace HullDesign1
{
    public sealed class ShipHullSurface
    {
        public delegate Point3D Function(double x, double z);
        private double xmin;//= -2;
        private double xmax;//= 2;
        private double ymin;//= 0;
        private double ymax;//= 5;
        private double zmin;//= -10;
        private double zmax;//= 10;
        private int nx = 30;
        private int nz = 30;
        private Color lineColor = Colors.Blue;
        private Color surfaceColor = Colors.Gray;
        private Color backSurfaceColor = Colors.Gray;
        private Point3D center = new Point3D();
        private bool isHiddenLine = false;
        private bool isWireframe = false;
        private Viewport3D viewport3d = new Viewport3D();


        private static volatile ShipHullSurface instance;
        private static object syncRoot = new Object();

        private ShipHullSurface() 
        {
   
         }

        public static ShipHullSurface Instance
       {
           get
           {
               if (instance == null)
               {
                   lock (syncRoot)
                   {
                       if (instance == null)
                           instance = new ShipHullSurface();
                   }
               }
               return instance;
           }
       }

 
        
        public bool IsWireframe
        {
            get { return isWireframe; }
            set { isWireframe = value; }
        }
        public bool IsHiddenLine
        {
            get { return isHiddenLine; }
            set { isHiddenLine = value; }
        }
        public Color LineColor
        {
            get { return lineColor; }
            set { lineColor = value; }
        }
        public Color SurfaceColor
        {
            get { return surfaceColor; }
            set { surfaceColor = value; }
        }
        public double Xmin
        {
            get { return xmin; }
            set { xmin = value; }
        }
        public double Xmax
        {
            get { return xmax; }
            set { xmax = value; }
        }
        public double Ymin
        {
            get { return ymin; }
            set { ymin = value; }
        }
        public double Ymax
        {
            get { return ymax; }
            set { ymax = value; }
        }
        public double Zmin
        {
            get { return zmin; }
            set { zmin = value; }
        }
        public double Zmax
        {
            get { return zmax; }
            set { zmax = value; }
        }
        public int Nx
        {
            get { return nx; }
            set { nx = value; }
        }
        public int Nz
        {
            get { return nz; }
            set { nz = value; }
        }
        public Point3D Center
        {
            get { return center; }
            set { center = value; }
        }
        public Viewport3D Viewport3d
        {
            get { return viewport3d; }
            set { viewport3d = value; }
        }
/// <summary>
/// /////////////////////
/// </summary>
/// <param name="f"></param>
     
        public void CreateSurface(Function f)
        {
            double dx = (Xmax - Xmin) / Nx;
            double dz = (Zmax - Zmin) / Nz;
            if (Nx < 2 || Nz < 2)
                return;
            Point3D[,] pts = new Point3D[Nx, Nz];
            for (int i = 0; i < Nx; i++)
            {
                double x = Xmin + i * dx;
                for (int j = 0; j < Nz; j++)
                {
                    double z = Zmin + j * dz;
                    pts[i, j] = f(x, z);
                    pts[i, j] += (Vector3D)Center;
                    pts[i, j] = Utility.GetNormalize(
                    pts[i, j], Xmin, Xmax,
                    Ymin, Ymax, Zmin, Zmax);
                }
            }
            Point3D[] p = new Point3D[4];
            for (int i = 0; i < Nx - 1; i++)
            {
                for (int j = 0; j < Nz - 1; j++)
                {
                    p[0] = pts[i, j];
                    p[1] = pts[i, j + 1];
                    p[2] = pts[i + 1, j + 1];
                    p[3] = pts[i + 1, j];
                    //Create rectangular face:
                    if (IsHiddenLine == false)
                        Utility.CreateRectangleFace(
                        p[0], p[1], p[2], p[3],
                        SurfaceColor, backSurfaceColor, Viewport3d);
                    // Create wireframe:
                    if (IsWireframe == true)
                        Utility.CreateWireframe(
                        p[0], p[1], p[2], p[3],
                        LineColor, Viewport3d);
                }
            }
        }

///////////////////////////////////////
        public bool CreateShipHullSurface(ref Viewport3D myViewport)
        {
            
            ////clean screen
            //ModelVisual3D m;
            //int ind;
            //for (ind = myViewport.Children.Count - 1; ind >= 6; ind--)
            //{
            //    m = (ModelVisual3D)myViewport.Children[ind];
            //    if (m.Content is DirectionalLight == false)
            //        myViewport.Children.Remove(m);
            //}


            Dictionary<string, List<point>> l_2DHullSections = new Dictionary<string, List<point>>();

            Sections.Instance.getPointHullSections(ref l_2DHullSections);
            //Check if all section - start from Deck
            foreach (KeyValuePair<string, List<point>> l_section in l_2DHullSections)
            {
                if (l_section.Value.First().y < l_section.Value.Last().y)
                {
                    l_section.Value.Reverse();
                }
            }


            double l_Z = 20;
            double l_dZ = -1.6;
            double l_ZPrev = l_Z - l_dZ;
   
            Point3D p0 = new Point3D();
            Point3D p1 = new Point3D();
            Point3D p2 = new Point3D();
            Point3D p3 = new Point3D();

            Point3D p0R = new Point3D();
            Point3D p1R = new Point3D();
            Point3D p2R = new Point3D();
            Point3D p3R = new Point3D();

            KeyValuePair<string, List<point>> l_prevSection = l_2DHullSections.First();                     
            List<point>.Enumerator l_prevSecEnumer = l_prevSection.Value.GetEnumerator();

            List<point>.Enumerator l_sectionEnumerator;

            l_prevSecEnumer.MoveNext();
            p0.X = l_prevSecEnumer.Current.x;
            if (p0.X < 0) { p0.X = -p0.X; }
            p0.Y = l_prevSecEnumer.Current.y;
            l_prevSecEnumer.MoveNext();

            List<point> l_prevSecPointsList = l_prevSection.Value;
            int i = 0;
            int secInd = 0;
            foreach (KeyValuePair<string, List<point>> l_section in l_2DHullSections)
            {
                if (secInd == 0)
                {
                    secInd++;
                    continue;
                }
                l_section.Value.Skip(1);
                l_sectionEnumerator = l_section.Value.GetEnumerator();
                //l_sectionEnumerator.MoveNext();
                foreach (point l_point2D in l_section.Value)
                {
                    if (i == 0)
                    {                    
                        p1.X = l_point2D.x;
                        if (p1.X < 0) { p1.X = -p1.X; }
                        p1.Y = l_point2D.y;
                        l_prevSecEnumer.MoveNext();
                        i++;
                        //continue;
                    }

                    p2.X = l_prevSecEnumer.Current.x; //l_prevSecPointsList[i].x;
                    if (p2.X < 0) { p2.X = -p2.X; }
                    p2.Y = l_prevSecEnumer.Current.y; //l_prevSecPointsList[i].y;
                    p3.X = l_point2D.x;
                    if (p3.X < 0) { p3.X = -p3.X; }
                    p3.Y = l_point2D.y;

                    p0.Z = p2.Z = l_ZPrev;
                    p1.Z = p3.Z = l_Z;

                    p0 += (Vector3D)Center;
                    p0 = Utility.GetNormalize(p0, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
                    p1 += (Vector3D)Center;
                    p1 = Utility.GetNormalize(p1, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
                    p2 += (Vector3D)Center;
                    p2 = Utility.GetNormalize(p2, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
                    p3 += (Vector3D)Center;
                    p3 = Utility.GetNormalize(p3, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);

                    p0R = p0;
                    p0R.X = -p0.X;
                    p1R = p1;
                    p1R.X = -p1.X;
                    p2R = p2;
                    p2R.X = -p2.X;
                    p3R = p3;
                    p3R.X = -p3.X;

                    if (IsHiddenLine == false)
                    {
                        Utility.CreateRectangleFace(p0, p1, p2, p3, surfaceColor, backSurfaceColor, myViewport);
                        Utility.CreateRectangleFace(p0R, p1R, p2R, p3R, backSurfaceColor, surfaceColor, myViewport);
                    }
                    if (IsWireframe == true)
                    {
                        Utility.CreateWireframe(p0, p1, p2, p3, lineColor, myViewport);
                        Utility.CreateWireframe(p0R, p1R, p2R, p3R, lineColor, myViewport);
                    }

                    p0 = p2;
                    p1 = p3;
                    if (!l_prevSecEnumer.MoveNext())
                    {
                        i = 0;
                        break;
                    }
                  
                }
                //break;
                
                l_prevSecEnumer = l_section.Value.GetEnumerator();
                i = 0;
                l_ZPrev = l_Z;
                l_Z += l_dZ;
            }

            //createWaterSurface(ref myViewport);
            return true;
        }


        public bool createWaterSurface(ref Viewport3D myViewport)
        {
            Point3D p0 = new Point3D();
            Point3D p1 = new Point3D();
            Point3D p2 = new Point3D();
            Point3D p3 = new Point3D();

            p0.X = -20;
            p1.X = -20;
            p2.X = 20;
            p3.X = 20;

            p0.Y = 3;
            p1.Y = 3;
            p2.Y = 3;
            p3.Y = 3;

            p0.Z = -70;
            p1.Z = 70;
            p2.Z = 70;
            p3.Z = -70;

            surfaceColor = Colors.White;
            lineColor = Colors.Cyan;

            p0 += (Vector3D)Center;
            p0 = Utility.GetNormalize(p0, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
            p1 += (Vector3D)Center;
            p1 = Utility.GetNormalize(p1, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
            p2 += (Vector3D)Center;
            p2 = Utility.GetNormalize(p2, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
            p3 += (Vector3D)Center;
            p3 = Utility.GetNormalize(p3, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);

            Utility.CreateRectangleFace(p0, p1, p2, p3, Colors.LightGray, Colors.LightBlue, myViewport);
            //Utility.CreateWireframe(p0, p1, p2, p3, lineColor, myViewport);

            return true;
        }


        public bool CreateHullMesh(ref Viewport3D myViewport)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            Material material = new DiffuseMaterial();
            GeometryModel3D geometry = new GeometryModel3D(mesh, material);
            Utility.createHullGeometry(ref geometry, surfaceColor, backSurfaceColor, myViewport);
            


            Dictionary<string, List<point>> l_2DHullSections = new Dictionary<string, List<point>>();

            //Sections.Instance.getPointHullSections(ref l_2DHullSections);
            HullDraft.Instance.getPointHullSections(ref l_2DHullSections);

            double l_Z = 20;
            double l_dZ = -1.35;
            double l_ZPrev = l_Z + l_dZ;

            Point3D p0 = new Point3D();
            Point3D p1 = new Point3D();
            Point3D p2 = new Point3D();
            Point3D p3 = new Point3D();

            Point3D p0R = new Point3D();
            Point3D p1R = new Point3D();
            Point3D p2R = new Point3D();
            Point3D p3R = new Point3D();

            KeyValuePair<string, List<point>> l_prevSection = l_2DHullSections.First();
            List<point>.Enumerator l_prevSecEnumer = l_prevSection.Value.GetEnumerator();

            List<point>.Enumerator l_sectionEnumerator;

            l_prevSecEnumer.MoveNext();
            p0.X = l_prevSecEnumer.Current.x;
            if (p0.X < 0) { p0.X = -p0.X; }
            p0.Y = l_prevSecEnumer.Current.y;
            l_prevSecEnumer.MoveNext();

            List<point> l_prevSecPointsList = l_prevSection.Value;
            int i = 0;
            int secInd = 0;
            foreach (KeyValuePair<string, List<point>> l_section in l_2DHullSections)
            {
                if (secInd == 0)
                {
                    secInd++;
                    continue;
                }
                l_section.Value.Skip(1);
                l_sectionEnumerator = l_section.Value.GetEnumerator();
                //l_sectionEnumerator.MoveNext();
                foreach (point l_point2D in l_section.Value)
                {
                    if (i == 0)
                    {
                        p1.X = l_point2D.x;
                        if (p1.X < 0) { p1.X = -p1.X; }
                        p1.Y = l_point2D.y;
                        l_prevSecEnumer.MoveNext();
                        i++;
                        //continue;
                    }

                    p2.X = l_prevSecEnumer.Current.x; //l_prevSecPointsList[i].x;
                    if (p2.X < 0) { p2.X = -p2.X; }
                    p2.Y = l_prevSecEnumer.Current.y; //l_prevSecPointsList[i].y;
                    p3.X = l_point2D.x;
                    if (p3.X < 0) { p3.X = -p3.X; }
                    p3.Y = l_point2D.y;

                    p0.Z = p2.Z = l_ZPrev;
                    p1.Z = p3.Z = l_Z;

                    p0 += (Vector3D)Center;
                    p0 = Utility.GetNormalize(p0, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
                    p1 += (Vector3D)Center;
                    p1 = Utility.GetNormalize(p1, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
                    p2 += (Vector3D)Center;
                    p2 = Utility.GetNormalize(p2, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
                    p3 += (Vector3D)Center;
                    p3 = Utility.GetNormalize(p3, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);

                    p0R = p0;
                    p0R.X = -p0.X;
                    p1R = p1;
                    p1R.X = -p1.X;
                    p2R = p2;
                    p2R.X = -p2.X;
                    p3R = p3;
                    p3R.X = -p3.X;

                    if (IsHiddenLine == false)
                    {
                        Utility.addRectangleFaceToMesh(ref mesh, p0, p1, p2, p3, surfaceColor, backSurfaceColor, myViewport);
                        
                    }

                    p0 = p2;
                    p1 = p3;
                    if (!l_prevSecEnumer.MoveNext())
                    {
                        i = 0;
                        break;
                    }

                }
                //break;

                l_prevSecEnumer = l_section.Value.GetEnumerator();
                i = 0;
                l_ZPrev = l_Z;
                l_Z += l_dZ;
            }

            //createWaterSurface(ref myViewport);

             return true;

        }


        public bool CreateShipHullSurfaceFromHullDraft(ref Viewport3D myViewport)
        {

            Dictionary<string, List<point>> l_2DHullSections = new Dictionary<string, List<point>>();
            //Dictionary<string, List<point>> l_2DHullSections = new Dictionary<string, List<point>>();

            HullDraft.Instance.getPointHullSections(ref l_2DHullSections);
            //Section points in HullDraft are starting from Keel. 
            //Need to reverse sections point -- to start from Deck
            foreach (KeyValuePair<string, List<point>> l_section in l_2DHullSections)
            {
                if (l_section.Value.First().y < l_section.Value.Last().y)
                {
                    l_section.Value.Reverse();
                }
            }

         
            
            double l_Z = 70;
            double l_dZ = -1.6;
            double l_ZPrev = l_Z - l_dZ;

            Point3D p0 = new Point3D();
            Point3D p1 = new Point3D();
            Point3D p2 = new Point3D();
            Point3D p3 = new Point3D();

            Point3D p0R = new Point3D();
            Point3D p1R = new Point3D();
            Point3D p2R = new Point3D();
            Point3D p3R = new Point3D();

            KeyValuePair<string, List<point>> l_prevSection = l_2DHullSections.First();
            List<point>.Enumerator l_prevSecEnumer = l_prevSection.Value.GetEnumerator();

            point lastPointPrevSec = l_prevSection.Value.Last();

            List<point>.Enumerator l_sectionEnumerator;

            l_prevSecEnumer.MoveNext();
            p0.X = l_prevSecEnumer.Current.x;
            if (p0.X < 0) { p0.X = -p0.X; }
            p0.Y = l_prevSecEnumer.Current.y;
            l_prevSecEnumer.MoveNext();

            List<point> l_prevSecPointsList = l_prevSection.Value;
            int i = 0;
            int secInd = 0;
            int countElemInSection;
            int countElemInPrevSection = l_prevSection.Value.Count;
            foreach (KeyValuePair<string, List<point>> l_section in l_2DHullSections)
            {
                countElemInSection = l_section.Value.Count;
                if (secInd == 0)
                {
                    secInd++;
                    continue;
                }
                l_section.Value.Skip(1);
                l_sectionEnumerator = l_section.Value.GetEnumerator();
                //l_sectionEnumerator.MoveNext();
                bool lastElem = false;
                int pointsCount = 0;
                foreach (point l_point2D in l_section.Value)
                {
                    if (i == 0)
                    {
                        p1.X = l_point2D.x;
                        if (p1.X < 0) { p1.X = -p1.X; }
                        p1.Y = l_point2D.y;
                        l_prevSecEnumer.MoveNext();
                        i++;
                        //continue;
                    }

                    //Check if curent or previous section points are last
                    if ( (countElemInPrevSection != countElemInSection) &&
                        ((pointsCount == countElemInPrevSection - 1) || (pointsCount == countElemInSection - 1)))
                    {
                        //last elem in Prev section --> set current section to last elem and exit from loop of section
                        p2.X = lastPointPrevSec.x;
                        p2.Y = lastPointPrevSec.y; 
                        p3.X = l_section.Value.Last().x;
                        p3.Y = l_section.Value.Last().y;
                        lastElem = true; 
                    }
                    else
                    {
                        p2.X = l_prevSecEnumer.Current.x; //l_prevSecPointsList[i].x;                      
                        p2.Y = l_prevSecEnumer.Current.y; //l_prevSecPointsList[i].y;
                        p3.X = l_point2D.x;                        
                        p3.Y = l_point2D.y;

                    }
                    if (p2.X < 0) { p2.X = -p2.X; }
                    if (p3.X < 0) { p3.X = -p3.X; }

                    p0.Z = p2.Z = l_ZPrev;
                    p1.Z = p3.Z = l_Z;

                    p0 += (Vector3D)Center;
                    p0 = Utility.GetNormalize(p0, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
                    p1 += (Vector3D)Center;
                    p1 = Utility.GetNormalize(p1, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
                    p2 += (Vector3D)Center;
                    p2 = Utility.GetNormalize(p2, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
                    p3 += (Vector3D)Center;
                    p3 = Utility.GetNormalize(p3, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);

                    p0R = p0;
                    p0R.X = -p0.X;
                    p1R = p1;
                    p1R.X = -p1.X;
                    p2R = p2;
                    p2R.X = -p2.X;
                    p3R = p3;
                    p3R.X = -p3.X;

                    if (IsHiddenLine == false)
                    {
                        Utility.CreateRectangleFace(p0, p1, p2, p3, surfaceColor, backSurfaceColor, myViewport);
                        Utility.CreateRectangleFace(p0R, p1R, p2R, p3R, backSurfaceColor, surfaceColor, myViewport);
                    }
                    if (IsWireframe == true)
                    {
                        Utility.CreateWireframe(p0, p1, p2, p3, lineColor, myViewport);
                        Utility.CreateWireframe(p0R, p1R, p2R, p3R, lineColor, myViewport);
                    }

                    p0 = p2;
                    p1 = p3;
                    if (!l_prevSecEnumer.MoveNext())
                    {
                        i = 0;
                        break;
                    }

                    if (lastElem)
                        break;

                    pointsCount++;
                }
                //break;

                l_prevSecEnumer = l_section.Value.GetEnumerator();
                lastPointPrevSec = l_section.Value.Last();
                i = 0;
                l_ZPrev = l_Z;
                l_Z += l_dZ;
                countElemInPrevSection = countElemInSection;               

            }

            //createWaterSurface(ref myViewport);
            CreateAhtershteven(ref myViewport, ref l_2DHullSections, ( l_Z - l_dZ));
            return true;
        }

        private void CreateAhtershteven(ref Viewport3D myViewport, ref Dictionary<string, List<point>> i_2DHullSections, double i_Z)
        {
            //get last section
            List<point> SternSection = new List<point>();
            SternSection = i_2DHullSections.Last().Value;
            if (SternSection.First().y < SternSection.Last().y)
            {
                SternSection.Reverse();
            }

            Point3D p0 = new Point3D();
            Point3D p1 = new Point3D();
            Point3D p2 = new Point3D();
            Point3D p3 = new Point3D();

              //Create surface
            p0.X = SternSection.First().x;
            if (p0.X < 0) { p0.X = -p0.X; }
            p0.Y = SternSection.First().y;
            p1.X = -p0.X;
            p1.Y = p0.Y;
            SternSection.Skip(1);

            

            foreach (point l_point2D in SternSection)
            {
                p2.X = l_point2D.x;
                if (p2.X < 0) { p2.X = -p2.X; }
                p2.Y = l_point2D.y;
                p3.X = -p2.X;
                p3.Y = p2.Y;

                p0.Z = p1.Z = p2.Z = p3.Z = i_Z;

                p0 += (Vector3D)Center;
                p0 = Utility.GetNormalize(p0, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
                p1 += (Vector3D)Center;
                p1 = Utility.GetNormalize(p1, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
                p2 += (Vector3D)Center;
                p2 = Utility.GetNormalize(p2, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
                p3 += (Vector3D)Center;
                p3 = Utility.GetNormalize(p3, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);

                if (IsHiddenLine == false)
                {
                    Utility.CreateRectangleFace(p0, p1, p2, p3, Colors.Red, Colors.Red, myViewport);
                }
                if (IsWireframe == true)
                {
                    Utility.CreateWireframe(p0, p1, p2, p3, Colors.Blue, myViewport);
                }
                p0 = p2;
                p1 = p3;
            }

        }


        public void drawShipHullSurface(Viewport3D myViewport)
        {
            double e = 2;
            Xmax = e;
            Xmin = -e;
            Ymax = e + 3;
            Ymin = -e + 3;
            Zmax = e * 2;
            Zmin = -e * 2;

            //CreateShipHullSurfaceFromHullDraft(ref myViewport);
            CreateShipHullSurfaceFromHullDraft_New(ref myViewport);
            createWaterSurface(ref myViewport);
            CreateForshteven(ref myViewport);
        }

//-----------------------------------------------------------
        public bool CreateForshteven(ref Viewport3D myViewport)
        {         
            List<Point3D> sectionPoints = new List<Point3D>();
            Sections.Instance.getPoints3DBySecNumByWL(ref sectionPoints, 0);

            List<Point3D> ForshtevenPoints = new List<Point3D>();
            Edges.Instance.getForshtevenPointsByTWL(ref ForshtevenPoints);

            //Section points in HullDraft are starting from Keel. 
            //Reverse sections point -- to start from Deck
            if (sectionPoints.First().Y < sectionPoints.Last().Y)
            {
                    sectionPoints.Reverse();
            }
            double fistSec_Z = sectionPoints[0].Z; //??? 70

            Dictionary<string, List<Point3D>> dicWaterlinesPoints = new Dictionary<string, List<Point3D>>();
            Waterlines.Instance.getPointsHullWaterlines(ref dicWaterlinesPoints);

            //build list of watrelines ordinates
            List<double> waterlinesOrdinates = new List<double>();
            foreach (List<Point3D> l_waterline in dicWaterlinesPoints.Values)
            {
                waterlinesOrdinates.Add(l_waterline[0].Y);
            }
            //rebuild first section points by WLs ordinates - preparing to mesh build
            Sections.Instance.rebuildSectionPointsByWLOrdinates(ref sectionPoints, waterlinesOrdinates);
         
            //create list of Forsheteven points - from Waterlines
            List<Point3D> forshtevenPoints3D = new List<Point3D>();
            foreach (KeyValuePair<string, List<Point3D>> l_waterline3D in dicWaterlinesPoints)
            {
                l_waterline3D.Value.Reverse();
                if (l_waterline3D.Value[0].Z < fistSec_Z)
                    continue;
                Point3D l_newPoint = new Point3D(l_waterline3D.Value[0].X, l_waterline3D.Value[0].Y, l_waterline3D.Value[0].Z);
                forshtevenPoints3D.Add(l_newPoint);
            }
            Point3D lastPointOfForshteven = forshtevenPoints3D.Last();

            List<Point3D>.Enumerator sectionEnumer = sectionPoints.GetEnumerator();
            if (!sectionEnumer.MoveNext())
            {
                return false;
            }
            Point3D lastSectionPoint = sectionPoints.Last();
            

            Point3D p0 = new Point3D();
            Point3D p1 = new Point3D();
            Point3D p2 = new Point3D();
            Point3D p3 = new Point3D();

            Point3D p0R = new Point3D();
            Point3D p1R = new Point3D();
            Point3D p2R = new Point3D();
            Point3D p3R = new Point3D();

            int i = 0;
            int countForshteven = forshtevenPoints3D.Count();
            int countSection = sectionPoints.Count();

            p0 = forshtevenPoints3D.First();
            if (p0.X < 0) { p0.X = -p0.X; }
            forshtevenPoints3D.Skip(1);


            foreach (Point3D l_point in forshtevenPoints3D)
            {
                p2.X = l_point.X;
                p2.Y = l_point.Y;
                p2.Z = l_point.Z;
                p3.X = sectionEnumer.Current.X;
                p3.Y = sectionEnumer.Current.Y;
                p3.Z = sectionEnumer.Current.Z;

                if (p2.X < 0) { p2.X = -p2.X; }
                if (p3.X < 0) { p3.X = -p3.X; }

                p0.Z = p2.Z;
                p1.Z = p3.Z;

                p0 += (Vector3D)Center;
                p0 = Utility.GetNormalize(p0, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
                p1 += (Vector3D)Center;
                p1 = Utility.GetNormalize(p1, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
                p2 += (Vector3D)Center;
                p2 = Utility.GetNormalize(p2, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
                p3 += (Vector3D)Center;
                p3 = Utility.GetNormalize(p3, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);

                p0R = p0;
                p0R.X = -p0.X;
                p1R = p1;
                p1R.X = -p1.X;
                p2R = p2;
                p2R.X = -p2.X;
                p3R = p3;
                p3R.X = -p3.X;

                if (IsHiddenLine == false)
                {
                    Utility.CreateRectangleFace(p0, p1, p2, p3, Colors.Yellow, Colors.Red, myViewport);
                    Utility.CreateRectangleFace(p0R, p1R, p2R, p3R, Colors.Yellow, Colors.Red, myViewport);
                }
                if (IsWireframe == true)
                {
                    Utility.CreateWireframe(p0, p1, p2, p3, Colors.Yellow, myViewport);
                    Utility.CreateWireframe(p0R, p1R, p2R, p3R, Colors.Yellow, myViewport);
                }

                p0 = p2;
                p1 = p3;
                if (!sectionEnumer.MoveNext())
                {
                    i = 0;
                    break;
                }
            }

            return true;

        }

//------------------------------------------------------------
        public bool CreateShipHullSurfaceFromHullDraft_New(ref Viewport3D myViewport)
        {

            Dictionary<string, List<Point3D>> l_3DHullSections = new Dictionary<string, List<Point3D>>();

            Sections.Instance.getPoint3DHullSectionsByWL(ref l_3DHullSections);
            //Section points in HullDraft are starting from Keel. 
            //Need to reverse sections point -- to start from Deck
            foreach (KeyValuePair<string, List<Point3D>> l_section in l_3DHullSections)
            {
                if (l_section.Value.First().Y < l_section.Value.Last().Y)
                {
                    l_section.Value.Reverse();
                }
            }



            //double l_Z = 70;
            //double l_dZ = -1.6;
            //double l_ZPrev = l_Z - l_dZ;

            Point3D p0 = new Point3D();
            Point3D p1 = new Point3D();
            Point3D p2 = new Point3D();
            Point3D p3 = new Point3D();

            Point3D p0R = new Point3D();
            Point3D p1R = new Point3D();
            Point3D p2R = new Point3D();
            Point3D p3R = new Point3D();

            KeyValuePair<string, List<Point3D>> l_prevSection = new KeyValuePair<string, List<Point3D>>();
            l_prevSection = l_3DHullSections.First();
            List<Point3D>.Enumerator l_prevSecEnumer = l_prevSection.Value.GetEnumerator();

            Point3D lastPointPrevSec = l_prevSection.Value.Last();

            List<Point3D>.Enumerator l_sectionEnumerator;

            l_prevSecEnumer.MoveNext();
            p0.X = l_prevSecEnumer.Current.X;
            if (p0.X < 0) { p0.X = -p0.X; }
            p0.Y = l_prevSecEnumer.Current.Y;
            l_prevSecEnumer.MoveNext();

            List<Point3D> l_prevSecPointsList = l_prevSection.Value;
            int i = 0;
            int secInd = 0;
            int countElemInSection;
            int countElemInPrevSection = l_prevSection.Value.Count;
            foreach (KeyValuePair<string, List<Point3D>> l_section in l_3DHullSections)
            {
                countElemInSection = l_section.Value.Count;
                if (secInd == 0)
                {
                    secInd++;
                    continue;
                }
                l_section.Value.Skip(1);
                l_sectionEnumerator = l_section.Value.GetEnumerator();
                //l_sectionEnumerator.MoveNext();
                bool lastElem = false;
                int pointsCount = 0;
                foreach (Point3D l_point3D in l_section.Value)
                {
                    if (i == 0)
                    {
                        p1.X = l_point3D.X;
                        if (p1.X < 0) { p1.X = -p1.X; }
                        p1.Y = l_point3D.Y;
                        l_prevSecEnumer.MoveNext();
                        i++;
                        //continue;
                    }

                    //Check if curent or previous section points are last
                    if ((countElemInPrevSection != countElemInSection) &&
                        ((pointsCount == countElemInPrevSection - 1) || (pointsCount == countElemInSection - 1)))
                    {
                        //last elem in Prev section --> set current section to last elem and exit from loop of section
                        //p2.X = lastPointPrevSec.X;
                        //p2.Y = lastPointPrevSec.Y;
                        //p2.Z = lastPointPrevSec.Z;
                        p2 = lastPointPrevSec;
                        p3.X = l_section.Value.Last().X;
                        p3.Y = l_section.Value.Last().Y;
                        p3.Z = l_section.Value.Last().Z;
                        lastElem = true;
                    }
                    else
                    {
                        p2.X = l_prevSecEnumer.Current.X;                 
                        p2.Y = l_prevSecEnumer.Current.Y;
                        p2.Z = l_prevSecEnumer.Current.Z;
                        //p3.X = l_point3D.X;
                        //p3.Y = l_point3D.Y;
                        //p3.Z = l_point3D.Z;
                        p3 = l_point3D;

                    }
                    if (p2.X < 0) { p2.X = -p2.X; }
                    if (p3.X < 0) { p3.X = -p3.X; }

                    p0.Z = p2.Z;// = l_ZPrev;
                    p1.Z = p3.Z;// = l_Z;

                    p0 += (Vector3D)Center;
                    p0 = Utility.GetNormalize(p0, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
                    p1 += (Vector3D)Center;
                    p1 = Utility.GetNormalize(p1, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
                    p2 += (Vector3D)Center;
                    p2 = Utility.GetNormalize(p2, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);
                    p3 += (Vector3D)Center;
                    p3 = Utility.GetNormalize(p3, Xmin, Xmax, Ymin, Ymax, Zmin, Zmax);

                    p0R = p0;
                    p0R.X = -p0.X;
                    p1R = p1;
                    p1R.X = -p1.X;
                    p2R = p2;
                    p2R.X = -p2.X;
                    p3R = p3;
                    p3R.X = -p3.X;

                    if (IsHiddenLine == false)
                    {
                        Utility.CreateRectangleFace(p0, p1, p2, p3, surfaceColor, backSurfaceColor, myViewport);
                        Utility.CreateRectangleFace(p0R, p1R, p2R, p3R, backSurfaceColor, surfaceColor, myViewport);
                    }
                    if (IsWireframe == true)
                    {
                        Utility.CreateWireframe(p0, p1, p2, p3, lineColor, myViewport);
                        Utility.CreateWireframe(p0R, p1R, p2R, p3R, lineColor, myViewport);
                    }

                    p0 = p2;
                    p1 = p3;
                    if (!l_prevSecEnumer.MoveNext())
                    {
                        i = 0;
                        break;
                    }

                    if (lastElem)
                        break;

                    pointsCount++;
                }
                //break;

                l_prevSecEnumer = l_section.Value.GetEnumerator();
                lastPointPrevSec = l_section.Value.Last();
                i = 0;
                //l_ZPrev = l_Z;
                //l_Z += l_dZ;
                countElemInPrevSection = countElemInSection;

            }

            //createWaterSurface(ref myViewport);
            //CreateAhtershteven(ref myViewport, ref l_2DHullSections, (l_Z - l_dZ));
            return true;
        }

    
    
    
    }
}
