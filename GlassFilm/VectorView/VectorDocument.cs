﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using Svg;
using Svg.Transforms;
using Svg.Pathing;
using System.IO;
using System.Xml;
using System.Drawing.Drawing2D;

namespace VectorView
{
    public enum VectorFitStyle
    {
        None, 
        Vertical,
        Horizontal,
        Both
    }

    public class VectorDocument
    {
        List<VectorPath> paths = new List<VectorPath>();
        Color normalLineColor = Color.LightBlue;
        Color docLimitLineColor = Color.Red;
        Color selectedLineColor = Color.OrangeRed;

        Color rullerBorderColor = Color.LightGray;
        Color rullerBackColor = Color.WhiteSmoke;

        bool showDocumentLimit = false;
        bool showRuller = false;
        float rullerWidth = 22f;

        float width = 0;
        float height = 0;

        float offsetX = 0;
        float offsetY = 0;

        float scale = 1f;

        float dpi = 96;

        public List<VectorPath> Paths
        {
            get
            {
                return paths;
            }
        }

        public Color NormalLineColor
        {
            get
            {
                return normalLineColor;
            }

            set
            {
                normalLineColor = value;
            }
        }

        Pen AdjustPen(Pen p)
        {
            p.Width = 1 / scale;
            return p;
        }

        Pen normalLinePen = null;
        public Pen NormalLinePen
        {
            get
            {
                if (normalLinePen == null || normalLinePen.Color != normalLineColor)
                    normalLinePen = new Pen(normalLineColor);

                return AdjustPen(normalLinePen);
            }
        }

        Pen selectedLinePen = null;
        public Pen SelectedLinePen
        {
            get
            {
                if (selectedLinePen == null || selectedLinePen.Color != selectedLineColor)
                    selectedLinePen = new Pen(selectedLineColor);

                return AdjustPen(selectedLinePen);
            }
        }

        public float Scale
        {
            get
            {
                return scale;
            }

            set
            {
                scale = value;
            }
        }

        public bool ShowRuller
        {
            get
            {
                return showRuller;
            }

            set
            {
                showRuller = value; 
            }
        }

        public bool ShowDocumentLimit
        {
            get
            {
                return showDocumentLimit;
            }

            set
            {
                showDocumentLimit = value;
            }
        }

        public float Width
        {
            get
            {
                return width;
            }

            set
            {
                width = value;
            }
        }

        public float Height
        {
            get
            {
                return height;
            }

            set
            {
                height = value;
            }
        }

        public float OffsetX
        {
            get
            {
                return offsetX;
            }

            set
            {
                offsetX = value;
            }
        }

        public float OffsetY
        {
            get
            {
                return offsetY;
            }

            set
            {
                offsetY = value;
            }
        }

        public VectorPath CreatePath()
        {
            VectorPath p = new VectorPath(this);
            paths.Add(p);
            return p;
        }

        public void DrawPath(Graphics g, Pen pen, VectorPath path, PointF location, float scale, float angle)
        {
            GraphicsPath p = path.CopyPath();
            Matrix mt = new Matrix();

            RectangleF r = p.GetBounds();
            PointF center = new PointF(r.X + r.Width / 2, r.Y + r.Height / 2);

            mt.Translate(-center.X, -center.Y);
            p.Transform(mt);

            mt.Reset();
            mt.Rotate(angle);
            p.Transform(mt);

            mt.Reset();
            mt.Scale(scale, scale);
            p.Transform(mt);

            mt.Translate(location.X, location.Y);
            p.Transform(mt);

            r = p.GetBounds();

            if (r.Width >= 1 && r.Height >= 1)
                g.DrawPath(pen, p);
        }

        void BeginRender(Graphics g)
        {
            g.ResetTransform();

            if (normalLinePen == null)
                normalLinePen = new Pen(normalLineColor, 1);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        }

        void DrawRuller(Graphics g)
        {
            Pen rp = new Pen(rullerBorderColor, 0.1f);
            SolidBrush rb = new SolidBrush(rullerBackColor);

            g.FillRectangle(rb, 0, 0, g.ClipBounds.Right, rullerWidth);
            g.FillRectangle(rb, 0, 0, rullerWidth, g.ClipBounds.Bottom);

            g.DrawLine(rp, rullerWidth, rullerWidth, g.ClipBounds.Right, rullerWidth);
            g.DrawLine(rp, rullerWidth, rullerWidth, rullerWidth, g.ClipBounds.Bottom);

            float sx = (offsetX + rullerWidth);
            float sy = (offsetY + rullerWidth);

            float wx = g.ClipBounds.Width / scale;
            float wy = g.ClipBounds.Height / scale;

            g.DrawRectangle(Pens.Orange, sx, sy, g.ClipBounds.Width, g.ClipBounds.Height);
        }

        public void Render(Graphics g)
        {
            BeginRender(g);

            float ox = 0, oy = 0;

            if (showRuller)
            {
                ox = rullerWidth;
                oy = rullerWidth;
            }

            ox += offsetX;
            oy += offsetY;

            if (scale <= 0)
                scale = 0.001f;

            g.TranslateTransform(ox, oy);
            g.ScaleTransform(scale, scale);

            if (Debugger.IsAttached)
            {
            }

            foreach (VectorPath p in paths)
            {
                p.Render(g);
            }
          
            if (showDocumentLimit)
            {
                RectangleF r = new RectangleF(-2, -2, width + 4, height + 4);

                Pen bp = new Pen(docLimitLineColor, normalLinePen.Width);
                g.DrawRectangle(bp, r.X, r.Y, r.Width, r.Height);
            }

            //DrawPath(g, paths[2], new PointF(1000, 1000), 0.5f);

            g.ResetTransform();

            if (showRuller)
            {
                DrawRuller(g);
            }
        }

        public string ToHPGL()
        {
            return ToHPGL(null, null);
        }

        public string ToHPGL(string sendBefore, string sendAfter)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("IN;\nIP;\nSP1;\nPA;\n");

            if (sendBefore != null)
                sb.Append(sendBefore);

            foreach (VectorPath s in paths)
            {
                sb.Append(s.ToHPGL());
                sb.Append("\n");
            }

            if (sendAfter != null)
                sb.Append(sendAfter);

            sb.Append("PU0,0\nSP;");

            return sb.ToString();
        }

        void ParseSvgElement(SvgElement el)
        {
            if (el is SvgPath)
            {
                SvgPath p = (SvgPath)el;
                VectorPath s = CreatePath();

                foreach (SvgPathSegment seg in p.PathData)
                {
                    if (seg is SvgLineSegment)
                    {
                        s.LineTo(seg.End.X, seg.End.Y);
                    }
                    else if (seg is SvgCubicCurveSegment)
                    {
                        SvgCubicCurveSegment q = (SvgCubicCurveSegment)seg;
                        s.CurveTo(q.End.X, q.End.Y, q.FirstControlPoint.X, q.FirstControlPoint.Y, q.SecondControlPoint.X, q.SecondControlPoint.Y);
                    }
                    else if (seg is SvgQuadraticCurveSegment)
                    {
                        SvgQuadraticCurveSegment q = (SvgQuadraticCurveSegment)seg;
                        s.QCurveTo(q.End.X, q.End.Y, q.ControlPoint.X, q.ControlPoint.Y);
                    }
                    else if (seg is SvgClosePathSegment)
                    {
                        s.ClosePath();
                    }
                    else if (seg is SvgMoveToSegment)
                    {
                        s.MoveTo(seg.End.X, seg.End.Y);
                    }
                    else
                    {

                    }
                }
            }
            else
            {

            }

            foreach (SvgElement n in el.Children)
            {
                ParseSvgElement(n);
            }
        }

        public void AdjustSizeToContent()
        {
            RectangleF r = GetBoundRect();

            width = r.Right;
            height = r.Bottom;
        }

        public void LoadSVGFromFile(string file)
        {
            string svg = File.ReadAllText(file, Encoding.UTF8);
            LoadSVG(svg);
        }

        public void LoadSVG(string svg)
        {
            paths.Clear();

            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(svg);
            SvgDocument doc = SvgDocument.Open(xdoc);

            dpi = doc.Ppi;

            foreach (SvgElement e in doc.Children)
            {
                ParseSvgElement(e);
            }

            AdjustSizeToContent();
        }

        public RectangleF GetBoundRect()
        {
            return GetBoundRect(false);
        }

        RectangleF GetBoundRect(bool selection)
        {
            float minx, miny, maxx, maxy;

            minx = float.MaxValue;
            miny = float.MaxValue;
            maxx = float.MinValue;
            maxy = float.MinValue;

            foreach (VectorPath e in paths)
            {
                if (selection && !e.IsSelected)
                    continue;

                RectangleF r = e.GetBoundRect();

                minx = Math.Min(minx, r.X);
                miny = Math.Min(miny, r.Y);
                maxx = Math.Max(maxx, r.Right);
                maxy = Math.Max(maxy, r.Bottom);
            }

            return new RectangleF(minx, miny, maxx - minx, maxy - miny);
        }

        public VectorPath GetPathOnPoint(PointF p)
        {
            foreach (VectorPath path in paths)
            {
                if (path.IsPointInside(p))
                    return path;
            }

            return null;
        }

        public string ToSVG()
        {
            StringBuilder sb = new StringBuilder();

            RectangleF r = Rectangle.Round(GetBoundRect());
            sb.AppendFormat("<svg xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:cc=\"http://creativecommons.org/ns#\" xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:svg=\"http://www.w3.org/2000/svg\" xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"{0} {1} {2} {3}\" height=\"{3}\" width=\"{2}\" version=\"1.1\">\n", r.X, r.Y, r.Width, r.Height);

            foreach (VectorPath s in paths)
            {
                sb.Append("   " + s.ToSVGPath());
                sb.Append("\n");
            }

            sb.Append("</svg>");
            return sb.ToString();
        }

        public void SaveToSVGFile(string path)
        {
            string svg = ToSVG();
            File.WriteAllText(path, svg);
        }

        public void DrawRect(Graphics g, RectangleF r, Pen pen)
        {
            pen.Width = 1 / scale;
            g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
        }

        public void DrawPoint(Graphics g, PointF p, float width = 3)
        {
            float w = width / scale;
            g.FillEllipse(Brushes.OrangeRed, p.X - w / 2, p.Y - w / 2, w, w);
        }

        public void FillRect(Graphics g, RectangleF r, Color c)
        {
            g.FillRectangle(new SolidBrush(c), r.X, r.Y, r.Width, r.Height);
        }

        public PointF ViewPointToDocPoint(PointF vp)
        {
            float x, y;

            x = (vp.X - offsetX) / scale;
            y = (vp.Y - offsetY) / scale;

            return new PointF(x, y);
        }

        public PointF DocPointToViewPoint(PointF dp)
        {
            float x, y;

            x = dp.X  * scale + offsetX;
            y = dp.Y  * scale + offsetY;

            return new PointF(x, y);
        }

        public RectangleF DocRectToViewRect(RectangleF dr)
        {
            RectangleF r = new RectangleF();

            PointF vp = DocPointToViewPoint(new PointF(dr.X, dr.Y));
            r.X = vp.X;
            r.Y = vp.Y;
            vp = DocPointToViewPoint(new PointF(dr.Right, dr.Bottom));
            r.Width = vp.X - r.X;
            r.Height = vp.Y - r.Y;

            return r;
        }

        public List<VectorPath> GetPathsInsideRect(PointF p1, PointF p2)
        {
            List<VectorPath> ret = new List<VectorPath>();

            foreach (VectorPath p in paths)
            {
                PointF m = p.GetMiddlePoint();

                if ((m.X >= p1.X && m.X <= p2.X) && (m.Y >= p1.Y && m.Y <= p2.Y))
                {
                    ret.Add(p);
                }
            }

            return ret;
        }

        public VectorPath ImportPath(VectorPath p)
        {
            VectorPath d = CreatePath();

            foreach (VectorEdge e in p.Edges)
            {
                VectorEdge c = e.Clone();

                d.AddEdge(c);
            }

            return d;
        }

        public void AutoFit(Rectangle size, VectorFitStyle style, bool center, bool fitContent)
        {
            float margin = 0;

            RectangleF bb;
            if (paths.Count == 0 || !fitContent)
            {
                bb = new RectangleF(0, 0, width, height);
            }
            else
            {
                bb = GetBoundRect();
                bb.Width += bb.X;
                bb.Height += bb.Y;
            }

            bb.Inflate(margin, margin);

            if (showRuller)
            {
                size.X += (int)rullerWidth + 2;
                size.Y += (int)rullerWidth + 2;

                size.Width -= (int)rullerWidth + 2;
                size.Height -= (int)rullerWidth + 2;
            }

            float s = 1;

            switch (style)
            {
                case VectorFitStyle.None:
                    return;
                case VectorFitStyle.Vertical:
                    s = size.Height / bb.Height;
                    break;
                case VectorFitStyle.Horizontal:
                    s = size.Width / bb.Width;
                    break;
                case VectorFitStyle.Both:

                    float sv = size.Height / bb.Height;
                    float sh = size.Width / bb.Width;

                    s = Math.Min(sv, sh);

                    break;
            }

            scale = s;

            if (center)
            {
                offsetX = (size.Width - bb.Width * s) / 2;
                offsetY = (size.Height - bb.Height * s) / 2;

                if (showRuller)
                {
                    offsetX += rullerWidth;
                    offsetY += rullerWidth;
                }
            }
        }
    }
}