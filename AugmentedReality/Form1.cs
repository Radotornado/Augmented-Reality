using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;

namespace AugmentedReality
{
    public partial class AR : Form
    {
        Thread thread;
        int option;

        public AR()
        {
            InitializeComponent();
        }

        public void button1_Click(object sender, EventArgs e)
        {
            StartRecording();
        }

        public void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            thread.Abort();
            thread = null;
        }

        public void StartRecording()
        {
            option = int.Parse(cmbOption.Text);
            thread = new Thread(new ThreadStart(Run));
            thread.Start();
        }

        public void Run()
        {
            CvCapture cap = Cv.CreateCameraCapture(1);
            IplImage pic = new IplImage("rump.jpg");
            Cv.Flip(pic, pic, FlipMode.Y);


            int width = 5;
            int height = 4;
            int sqares = 20;
            CvSize size = new CvSize(width, height);

            CvMat wMatrix = Cv.CreateMat(3, 3, MatrixType.F32C1);
            CvPoint2D32f[] corners = new CvPoint2D32f[sqares];

            IplImage img;
            IplImage disp;
            IplImage cimg;
            IplImage nimg;

            int cornerCount;

            while (thread != null)
            {
                img = Cv.QueryFrame(cap);

                Cv.Flip(img, img, FlipMode.Y);

                disp = Cv.CreateImage(Cv.GetSize(img), BitDepth.U8, 3);
                cimg = Cv.CreateImage(Cv.GetSize(img), BitDepth.U8, 3);
                nimg = Cv.CreateImage(Cv.GetSize(img), BitDepth.U8, 3);

                IplImage gray = Cv.CreateImage(Cv.GetSize(img), img.Depth, 1);
                bool found = Cv.FindChessboardCorners(img, size, out corners, out cornerCount, ChessboardFlag.AdaptiveThresh | ChessboardFlag.FilterQuads);

                Cv.CvtColor(img, gray, ColorConversion.BgrToGray);

                CvTermCriteria criteria = new CvTermCriteria(CriteriaType.Epsilon, 30, 0.1);
                Cv.FindCornerSubPix(gray, corners, cornerCount, new CvSize(11, 11), new CvSize(-1, -1), criteria);

                if (cornerCount == sqares)
                {
                    if (option == 1)
                    {
                        CvPoint2D32f[] p = new CvPoint2D32f[4];
                        CvPoint2D32f[] q = new CvPoint2D32f[4];

                        IplImage blank = Cv.CreateImage(Cv.GetSize(pic), BitDepth.U8, 3);

                        q[0].X = (float)pic.Width * 0;
                        q[0].Y = (float)pic.Height * 0;
                        q[1].X = (float)pic.Width;
                        q[1].Y = (float)pic.Height * 0;

                        q[2].X = (float)pic.Width;
                        q[2].Y = (float)pic.Height;
                        q[3].X = (float)pic.Width * 0;
                        q[3].Y = (float)pic.Height;

                        p[0].X = corners[0].X;
                        p[0].Y = corners[0].Y;
                        p[1].X = corners[4].X;
                        p[1].Y = corners[4].Y;

                        p[2].X = corners[19].X;
                        p[2].Y = corners[19].Y;
                        p[3].X = corners[15].X;
                        p[3].Y = corners[15].Y;

                        Cv.GetPerspectiveTransform(q, p, out wMatrix);

                        Cv.Zero(nimg);
                        Cv.Zero(cimg);

                        Cv.WarpPerspective(pic, nimg, wMatrix);
                        Cv.WarpPerspective(blank, cimg, wMatrix);
                        Cv.Not(cimg, cimg);

                        Cv.And(cimg, img, cimg);
                        Cv.Or(cimg, nimg, img);

                        Cv.Flip(img, img, FlipMode.Y);
                        Bitmap bm = BitmapConverter.ToBitmap(img);
                        bm.SetResolution(pictureBox1.Width, pictureBox1.Height);
                        pictureBox1.Image = bm;
                    }
                    else
                    {
                        CvPoint[] p = new CvPoint[4];

                        p[0].X = (int)corners[0].X;
                        p[0].Y = (int)corners[0].Y;
                        p[1].X = (int)corners[4].X;
                        p[1].Y = (int)corners[4].Y;

                        p[2].X = (int)corners[19].X;
                        p[2].Y = (int)corners[19].Y;
                        p[3].X = (int)corners[15].X;
                        p[3].Y = (int)corners[15].Y;

                        Cv.Line(img, p[0], p[1], CvColor.Red, 2);
                        Cv.Line(img, p[1], p[2], CvColor.Green, 2);
                        Cv.Line(img, p[2], p[3], CvColor.Blue, 2);
                        Cv.Line(img, p[3], p[0], CvColor.Yellow, 2);
                        
                        Cv.DrawChessboardCorners(img, size, corners, found);
                        Cv.Flip(img, img, FlipMode.Y);
                        Bitmap bm = BitmapConverter.ToBitmap(img);
                        bm.SetResolution(pictureBox1.Width, pictureBox1.Height);
                        pictureBox1.Image = bm;
                    }
                }
                else
                {
                    Cv.Flip(gray, gray, FlipMode.Y);
                    Bitmap bm = BitmapConverter.ToBitmap(gray);
                    bm.SetResolution(pictureBox1.Width, pictureBox1.Height);
                    pictureBox1.Image = bm;
                }
            }
        }
    }
}
