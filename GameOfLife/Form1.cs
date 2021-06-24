﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class Form1 : Form
    {
        const int healthCondition1 = 2; // two adjacent dots required to survive
        const int healthCondition2 = 3; // three adjacent dots required to grow

        static int speed = 20; // time in ms between cycles

        const int WidthX = 345; // X dimension of cell grid
        const int WidthY = 185; // Y dimension of cell grid

        const int Xoffset = 160; //X offset from upper left corner of window
        const int Yoffset = 40;

        const int gridSize = 5; // distance between grids

        const int cellSize = 4; //radius of dots


        uint drawMode = 0;

        Bitmap bmp = new Bitmap((WidthX * gridSize) + (gridSize), (WidthY * gridSize) + (gridSize), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);


        Stopwatch frameStopwatch = new Stopwatch();
        double frameCounter = 0;
        double drawAvg = 0;
        double calcAvg = 0;


        SolidBrush dotcolor = new SolidBrush(Color.LavenderBlush);
        SolidBrush backcolor = new SolidBrush(Color.DarkGoldenrod);
        SolidBrush shadowcolor = new SolidBrush(Color.LightSalmon);


        static bool[,] board = new bool[WidthX, WidthY];
        static bool[,] oldboard = new bool[WidthX, WidthY];
        static bool[,] oldoldboard = new bool[WidthX, WidthY];
        static bool[][,] rgbboardhistory = new bool[7][,]; //[ new bool[WidthX,WidthY]; // = new bool[,]>();
        static Color[] ShadowColors = new Color[] { Color.FromArgb(237,213,186),
                                                    Color.FromArgb(228,200,156),
                                                    Color.FromArgb(219,186,127),
                                                    Color.FromArgb(210,173,98),
                                                    Color.FromArgb(201,160,68),
                                                    Color.FromArgb(192,146,39),
                                                    Color.DarkGoldenrod

        };
        
        

        static Random rand = new Random();


        static Timer timer1 = new Timer();

        static Timer timer2 = new Timer();

        Bitmap image1;

        public Form1()
        {
            InitializeComponent();

            resizePicBox();

            using (var g = Graphics.FromImage(bmp))
            {

                Rectangle rect = new Rectangle(0, 0, (WidthX * gridSize) + 2*gridSize, (WidthY * gridSize) + 2*gridSize);

                // Fill rectangle to screen.
                g.FillRectangle(backcolor, rect);

                this.pictureBox1.Image = bmp;
            }
            
            

            for (int n = 0; n < 7; n++)
                rgbboardhistory[n] = new bool[WidthX, WidthY];



            for (int i = 0; i < WidthX; i++)
                for (int j = 0; j < WidthY; j++)
                {

                    board[i, j] = false;
                }

            for (int n = 0; n < rgbboardhistory.Length; n++)
                for (int i = 0; i < WidthX; i++)
                {
                    for (int j = 0; j < WidthY; j++)
                    {
                        rgbboardhistory[n][i, j] = false;
                    }
                }
            drawBoard();

            //createRandomBoard();

            timer1.Interval = speed;
            timer1.Tick += Timer1_Tick;

            timer2.Interval = 333;
            timer2.Tick += Timer2_Tick;

        }

        private void resizePicBox()
        {
            this.Size = new Size(WidthX * gridSize - 220, WidthY * gridSize - 120);

            pictureBox1.Width = (WidthX * gridSize);
            pictureBox1.Height = (WidthY * gridSize);
            pictureBox1.Refresh();

            //this.Width =  WidthX * gridSize;
            //this.Height = (WidthY * gridSize) - 30;
            
            //this.ClientSize = new Size(WidthX * gridSize, WidthY * gridSize);
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (drawMode >= 1)
            {
                for (int n = 6; n > 0; n--)
                {
                    rgbboardhistory[n] = rgbboardhistory[n - 1];
                }
                rgbboardhistory[0] = board;
            }

            oldboard = board;

            frameStopwatch.Start();
            //bool[,] oldboard = board;

           
            //bool[,] newboard = calculateNextBoard();
            //board = newboard;
            board = calculateNextBoard();

            frameStopwatch.Stop();
            calcAvg += (double)frameStopwatch.Elapsed.TotalMilliseconds;
            frameStopwatch.Reset();

            frameStopwatch.Start();
            if (drawMode == 0)
                drawChangedCells(oldboard, board);
            else if (drawMode >= 1 )
                drawChangedCellsShadowed(oldboard, board);
            //drawBoard();
            frameStopwatch.Stop();
            drawAvg += (double)frameStopwatch.Elapsed.TotalMilliseconds;
            frameStopwatch.Reset();
            
            frameCounter++;

            //drawBoard();
            
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {

            double drawavg = drawAvg / frameCounter;
            double calcavg = calcAvg / frameCounter;
            label5.Text = calcavg.ToString();
            label6.Text = drawavg.ToString();
            label4.Text = ((drawAvg + calcAvg) / frameCounter).ToString();
            frameCounter = 0;
            drawAvg = 0;
            calcAvg = 0;

            //drawBoard();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Stop();
                timer2.Stop();
            }
            else
            {
                //drawBoard();
                timer1.Start();
                timer2.Start();
            }
            
        }

        void createRandomBoard()
        {
            Random rand = new Random();
            for (int i = 0; i < WidthX-1; i++)
                for (int j = 0; j < WidthY-1; j++)
                {
                    bool rnd = rand.NextDouble() > 0.6;
                    board[i, j] = rnd;
                }

        }

        void drawBoard()
        {
            //Graphics myGraphics = base.CreateGraphics();
            // Pen myPen = new Pen(Color.Red);
            // Pen blankPen = new Pen(BackColor);
            //SolidBrush mySolidBrush = dotcolor;
            //SolidBrush myBlankBrush = backcolor;
            //myGraphics.DrawEllipse(myPen, 50, 50, 150, 150);

           
            using (var g = Graphics.FromImage(bmp))
            {
                for (int i = 0; i < WidthX; i++)
                    for (int j = 0; j < WidthY; j++)
                    {

                        if (board[i, j] == true)
                            GraphicsExtensions.FillRectangle(g, dotcolor, i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                        //GraphicsExtensions.FillCircle(myGraphics, mySolidBrush, Xoffset + i * gridSize, Yoffset + j * gridSize, circleSize);
                        else
                            GraphicsExtensions.FillRectangle(g, backcolor, i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                        //GraphicsExtensions.FillCircle(myGraphics, myBlankBrush, Xoffset + i * gridSize, Yoffset + j * gridSize, circleSize);
                    }
                this.pictureBox1.Image = bmp;
            }
            
        }



        void drawChangedCells(bool[,] oldboard, bool[,] Tempboard)
        {
            //Graphics myGraphics = base.CreateGraphics();
            // Pen myPen = new Pen(Color.Red);
            // Pen blankPen = new Pen(BackColor);
            //SolidBrush mySolidBrush = dotcolor;
            //SolidBrush myBlankBrush = backcolor;
            //myGraphics.DrawEllipse(myPen, 50, 50, 150, 150);

            //var bmp = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                for (int i = 0; i < WidthX; i++)
                {
                    for (int j = 0; j < WidthY; j++)
                    {

                        if ((oldboard[i, j] == false) && (Tempboard[i, j] == true))
                            GraphicsExtensions.FillRectangle(g, dotcolor, i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                        //GraphicsExtensions.FillCircle(myGraphics, mySolidBrush,);
                        else if ((oldboard[i, j] == true) && (Tempboard[i, j] == false))
                            GraphicsExtensions.FillRectangle(g, backcolor, i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                        //GraphicsExtensions.FillCircle(myGraphics, myBlankBrush, Xoffset + i * gridSize, Yoffset + j * gridSize, circleSize);
                    }
                }
            }
            this.pictureBox1.Image = bmp;
            //board = Tempboard;
        }


        void drawChangedCellsShadowed(bool[,] oldboard, bool[,] Tempboard)
        {
            //Graphics myGraphics = base.CreateGraphics();
            // Pen myPen = new Pen(Color.Red);
            // Pen blankPen = new Pen(BackColor);
            //SolidBrush mySolidBrush = dotcolor;
            //SolidBrush myBlankBrush = backcolor;
            //myGraphics.DrawEllipse(myPen, 50, 50, 150, 150);

            //var bmp = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                for (int i = 0; i < WidthX; i++)
                {
                    for (int j = 0; j < WidthY; j++)
                    {
                        for (int n = 1; n < 5; n++)
                        {
                            if ((rgbboardhistory[n][i, j] == true) && (rgbboardhistory[n + 1][i, j] == false))
                            {
                                if (drawMode == 1)
                                    GraphicsExtensions.FillRectangle(g, new SolidBrush(ShadowColors[n + 2]), i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                                else if (drawMode == 2)
                                    GraphicsExtensions.FillRectangle(g, new SolidBrush(ShadowColors[n + 1]), i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                                else if (drawMode == 3)
                                    GraphicsExtensions.FillRectangle(g, new SolidBrush(ShadowColors[n]), i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                            }
                        }

                        if ((oldboard[i, j] == true) && (Tempboard[i, j] == false))
                            GraphicsExtensions.FillRectangle(g, backcolor, i * gridSize + cellSize, j * gridSize + cellSize, cellSize);
                        else if ((oldboard[i, j] == false) && (Tempboard[i, j] == true))
                            GraphicsExtensions.FillRectangle(g, dotcolor, i * gridSize + cellSize, j * gridSize + cellSize, cellSize);

                        

                        
                       
                        //else if ((oldboard[i,j] == false))// && (oldboard[i,j] == false))
                            //GraphicsExtensions.FillRectangle(g, backcolor, i * gridSize + cellSize, j * gridSize + cellSize, cellSize);

                        

                       

                    }
                }
            }
            this.pictureBox1.Image = bmp;
            //board = Tempboard;
        }




        static bool[,] calculateNextBoard()
        {
            bool[,] Tempboard = new bool[WidthX, WidthY];

            //for (int i = 0; i < WidthX; i++)
             
            Parallel.For(0, WidthX, i =>
            {
                for (int j = 0; j < WidthY; j++)
                {
                    int willLive = 0;
                    if (j > 0) //upper row
                    {
                        if (i > 0) //left of
                            willLive += ToInt(board[i - 1, j - 1]);
                        willLive += ToInt(board[i, j - 1]);
                        if (i < WidthX - 1)
                            willLive += ToInt(board[i + 1, j - 1]);
                    }

                    if (i > 0) //same row
                        willLive += ToInt(board[i - 1, j]); //left of
                    if (i < WidthX - 1)
                        willLive += ToInt(board[i + 1, j]);

                    if (j < WidthY - 1)//lower row
                    {
                        if (i > 0) //left of
                            willLive += ToInt(board[i - 1, j + 1]);
                        willLive += ToInt(board[i, j + 1]);
                        if (i < WidthX - 1)
                            willLive += ToInt(board[i + 1, j + 1]);
                    }


                    if (board[i, j] == true)
                    {
                        if (willLive >= healthCondition1 && willLive <= healthCondition2)
                            Tempboard[i, j] = true;
                        else
                            Tempboard[i, j] = false;
                    }
                    else // if original cell was empty
                    {
                        if (willLive == healthCondition2)
                            Tempboard[i, j] = true;
                        else
                            Tempboard[i, j] = false;
                    }




                }
            });
            
            return Tempboard;
        }


      
            public static int ToInt(bool value)
            {
                return value ? 1 : 0;
            }

        private void button2_Click(object sender, EventArgs e)
        {
            //timer1.Stop();
            //createBoard();
            loadImagefromBMP("GOL1.bmp");
            if (!(timer1.Enabled))
                drawBoard();
        }


        private void loadImagefromBMP(string BMPname)
        {
            image1 = new Bitmap(BMPname, true);
            label1.Text = "BMP format: " + Environment.NewLine + image1.PixelFormat.ToString() + Environment.NewLine;
            label1.Text += image1.Height + "x" + image1.Width;

            int x, y;

            // Loop through the images pixels to reset color.
            for (x = 0; x < image1.Width; x++)
            {
                for (y = 0; y < image1.Height; y++)
                {
                    int pixelColor = image1.GetPixel(x, y).ToArgb();
                    int empty = Color.Empty.ToArgb();
                    Debug.WriteLine("Pixel: " + x + ":" + y + " - pixelcolor: " + pixelColor + " - empty: " + empty);
                    if (pixelColor < -65794)
                        board[x, y] = true;
                    else
                        board[x, y] = false;

                }
            }
        }

        private void loadImagetoPos(string BMPname, int Xoffset, int Yoffset)
        {
            image1 = new Bitmap(BMPname, true);
            label1.Text = "BMP format: " + Environment.NewLine + image1.PixelFormat.ToString() + Environment.NewLine;
            label1.Text += image1.Height + "x" + image1.Width;

            int x, y;

            // Loop through the images pixels to reset color.
            for (x = 0; x < image1.Width; x++)
            {
                for (y = 0; y < image1.Height; y++)
                {
                    int pixelColor = image1.GetPixel(x, y).ToArgb();
                    int empty = Color.Empty.ToArgb();
                    Debug.WriteLine("Pixel: " + x + ":" + y + " - pixelcolor: " + pixelColor + " - empty: " + empty);
                    if (pixelColor < -65794)
                        board[((Xoffset/gridSize)-(image1.Width/2)) +x, ((Yoffset/gridSize)-(image1.Height/2))+y] = true;
                    else
                        board[(Xoffset / gridSize) + x, (Yoffset / gridSize) + y] = false;

                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            createRandomBoard();
            if (!(timer1.Enabled))
                drawBoard();
        }


        //protected override void OnPaint(PaintEventArgs e)
        //{
            //base.OnPaint(e);

            //var bmp = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            

        //}

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = trackBar1.Value + 1;
            label3.Text = trackBar1.Value + " ms";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            loadImagefromBMP("GOL2.bmp");
            if (!(timer1.Enabled))
                drawBoard();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                drawMode = 0;
           
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
                drawMode = 1;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
                drawMode = 2;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
                drawMode = 3;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            loadImagefromBMP("GOL3.bmp");
            if (!(timer1.Enabled))
                drawBoard();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < WidthX; i++)
                for (int j = 0; j < WidthY; j++)
                {
                    
                    board[i, j] = false;
                }

            for (int n = 0; n < rgbboardhistory.Length; n++)
                for (int i = 0; i < WidthX; i++)
                {
                    for (int j = 0; j < WidthY; j++)
                    {
                        rgbboardhistory[n][i, j] = false;
                    }
                }
            drawBoard();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            Point coordinates = me.Location;
            Debug.WriteLine(coordinates.ToString());

            if (radioButton5.Checked)
                loadImagetoPos("GOL3.BMP", coordinates.X, coordinates.Y);
            else if (radioButton6.Checked)
                loadImagetoPos("GOL2.BMP", coordinates.X, coordinates.Y);
            else if (radioButton7.Checked)
                loadImagetoPos("GOL1.BMP", coordinates.X, coordinates.Y);
            else if (radioButton8.Checked)
                loadImagetoPos("GOL4.BMP", coordinates.X, coordinates.Y);
            else if (radioButton9.Checked)
                loadImagetoPos("119P4H1V0.BMP", coordinates.X, coordinates.Y);
            else if (radioButton10.Checked)
                loadImagetoPos("GOL5.BMP", coordinates.X, coordinates.Y);
            else if (radioButton11.Checked)
                loadImagetoPos("GOL6.BMP", coordinates.X, coordinates.Y);

            if (!(timer1.Enabled))
                drawBoard();
            
           
        }
    }

    public static class GraphicsExtensions
    {
        public static void DrawCircle(this Graphics g, Pen pen,
                                      float centerX, float centerY, float radius)
        {
            g.DrawEllipse(pen, centerX - radius, centerY - radius,
                          radius + radius, radius + radius);
        }

        public static void FillCircle(this Graphics g, Brush brush,
                                      float centerX, float centerY, float radius)
        {
            g.FillEllipse(brush, centerX - radius, centerY - radius,
                          radius + radius, radius + radius);
        }

        public static void FillRectangle(this Graphics g, Brush brush, int x, int y, int size)
        {
            // Create rectangle.
            Rectangle rect = new Rectangle(x - (size / 2), y - (size /2), size, size);

            // Fill rectangle to screen.
            g.FillRectangle(brush, rect);
        }
        

    }
}
