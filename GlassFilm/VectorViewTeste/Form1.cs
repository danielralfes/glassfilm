﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VectorView;

namespace VectorViewTeste
{
    public partial class Form1 : Form
    {
        VectorViewCtr view = new VectorViewCtr();

        public Form1()
        {
            InitializeComponent();

            view.Parent = this;
            view.Visible = true;
            view.Width = 600;
            view.Height = 300;
            view.Left = 400;
            view.BackColor = Color.WhiteSmoke;
            view.Dock = DockStyle.Fill;

            view.DocementMoved += View_DocementMoved;
            view.SelectionMoved += View_SelectionMoved;
            view.SelectionTransformed += View_SelectionTransformed;
            view.SelectionChanged += View_SelectionChanged;

            //view.AllowScalePath = false;
            view.AllowMoveDocument = true;

            view.DoubleClick += View_DoubleClick;
        }

        private void View_DoubleClick(object sender, EventArgs e)
        {
            
        }

        private void Corte_DoubleClick(object sender, EventArgs e)
        {
            
        }

        private void View_SelectionChanged(object sender, VectorEventArgs e)
        {
            UpdateDocInfo();
        }

        private void View_SelectionTransformed(object sender, VectorEventArgs e)
        {
            UpdateDocInfo();
        }

        private void View_SelectionMoved(object sender, VectorEventArgs e)
        {
            UpdateDocInfo();
        }

        void UpdateDocInfo()
        {
            VectorDocument d = view.Document;



            if (d != null)
            {
                docInfo.Text = string.Format("Peças: {0}, Scala Visual: {1:0.00}, X: {2:0.00} Y: {3:0.00}", d.Paths.Count, d.Scale, d.OffsetX, d.OffsetY);
                RectangleF r = d.GetBoundRect(true);
                selInfo.Text = string.Format("Largura: {0:0.00}mm,  Altura: {1:0.00}mm Área {2:0.00} m²: ", r.Width, r.Height, view.GetSelectionArea() /* 0.000001f*/);
            }
        }

        private void View_DocementMoved(object sender, VectorEventArgs e)
        {
            UpdateDocInfo();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);

            e.Graphics.FillRectangle(Brushes.White, ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            view.AutoFit();
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Refresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1_Click(null, null);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            VectorDocument d = view.Document;
            d.LoadSVGFromFile(@"D:\teste.svg");
            view.AutoFit();
            view.GridSize = 10;
            view.Document.AutoCheckConstraints = true;
            //view.Document.ShowConvexHull = true;
            
            VectorPath vp = d.Paths[0];
            float area = vp.ComputeArea(false, 1);

            view.Document.CutSize = 1520;
            view.Document.ShowDocBorder = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string s = view.Document.ToHPGL();
            File.WriteAllText( @"D:\\teste.plt", s);
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
