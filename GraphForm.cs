using System;
using System.Drawing;
using System.Windows.Forms;

namespace LabWork
{
    public class GraphForm : Form
    {
        private const double xStart = 0.2;
        private const double xEnd = 10.0;
        private const double dx = 0.8;
        private Padding margin = new Padding(50);

        public GraphForm()
        {
            Text = "Resizable Graph — LabWork";
            BackColor = Color.White;
            DoubleBuffered = true;
            this.Resize += (s, e) => this.Invalidate();
            this.ClientSize = new Size(800, 600);
            this.MinimumSize = new Size(10, 10);
        }

        // додано явний OnResize щоб гарантувати перерисовку в будь-яких випадках
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle client = this.ClientRectangle;
            Rectangle plotRect = new Rectangle(margin.Left, margin.Top, client.Width - margin.Left - margin.Right, client.Height - margin.Top - margin.Bottom);
            
            // Перевірка на мінімальні розміри області графіка
            if (plotRect.Width <= 50 || plotRect.Height <= 50)
            {
                using (var font = new Font("Arial", 10))
                using (var brush = Brushes.Gray)
                {
                    string msg = "Збільште вікно";
                    var sz = g.MeasureString(msg, font);
                    g.DrawString(msg, font, brush, (client.Width - sz.Width) / 2, (client.Height - sz.Height) / 2);
                }
                return;
            }

            // compute y values
            var xs = new System.Collections.Generic.List<double>();
            var ys = new System.Collections.Generic.List<double>();

            // Замість накопичення double використовуємо к-сть кроків (менше похибок)
            int steps = (int)Math.Round((xEnd - xStart) / dx);
            if (steps < 0) steps = 0;
            for (int i = 0; i <= steps; i++)
            {
                double x = xStart + i * dx;
                // захист від випадків коли через округлення вийшли за межі
                if (x > xEnd + 1e-9) x = xEnd;
                double y = (x + Math.Cos(2 * x)) / (x + 2);
                xs.Add(x);
                ys.Add(y);
            }

            double xmin = xStart, xmax = xEnd;
            double ymin = double.PositiveInfinity, ymax = double.NegativeInfinity;
            foreach (var y in ys)
            {
                if (y < ymin) ymin = y;
                if (y > ymax) ymax = y;
            }
            if (Math.Abs(ymax - ymin) < 1e-9)
            {
                ymax = ymin + 1;
                ymin = ymin - 1;
            }

            var pts = new System.Collections.Generic.List<PointF>();
            for (int i = 0; i < xs.Count; i++)
            {
                double x = xs[i];
                double y = ys[i];
                float px = (float)(plotRect.Left + (x - xmin) / (xmax - xmin) * plotRect.Width);
                float py = (float)(plotRect.Bottom - ((y - ymin) / (ymax - ymin)) * plotRect.Height);
                pts.Add(new PointF(px, py));
            }

            // draw axes
            using (var axisPen = new Pen(Color.Black, 1))
            {
                float y0;
                if (ymin <= 0 && 0 <= ymax)
                    y0 = (float)(plotRect.Bottom - ((0 - ymin) / (ymax - ymin)) * plotRect.Height);
                else if (ymax < 0)
                    y0 = plotRect.Top;
                else
                    y0 = plotRect.Bottom;

                float x0;
                if (xmin <= 0 && 0 <= xmax)
                    x0 = (float)(plotRect.Left + ((0 - xmin) / (xmax - xmin)) * plotRect.Width);
                else if (xmax < 0)
                    x0 = plotRect.Right;
                else
                    x0 = plotRect.Left;

                g.DrawLine(axisPen, plotRect.Left, y0, plotRect.Right, y0);
                g.DrawLine(axisPen, x0, plotRect.Top, x0, plotRect.Bottom);

                using (var font = new Font("Segoe UI", 9))
                using (var brush = Brushes.Black)
                {
                    // x ticks and labels
                    for (double x = xmin; x <= xmax + 1e-9; x += dx)
                    {
                        float tx = (float)(plotRect.Left + (x - xmin) / (xmax - xmin) * plotRect.Width);
                        g.DrawLine(Pens.Black, tx, y0 - 4, tx, y0 + 4);
                        string s = x.ToString("0.##");
                        var sz = g.MeasureString(s, font);
                        g.DrawString(s, font, brush, tx - sz.Width / 2, y0 + 6);
                    }

                    // y ticks and labels
                    int ticks = 5;
                    for (int k = 0; k <= ticks; k++)
                    {
                        double yv = ymin + (ymax - ymin) * k / ticks;
                        float ty = (float)(plotRect.Bottom - (yv - ymin) / (ymax - ymin) * plotRect.Height);
                        g.DrawLine(Pens.Black, x0 - 4, ty, x0 + 4, ty);
                        string s = yv.ToString("0.###");
                        var sz = g.MeasureString(s, font);
                        g.DrawString(s, font, brush, x0 - sz.Width - 6, ty - sz.Height / 2);
                    }
                }
            }

            // draw polyline and points
            using (var pen = new Pen(Color.Blue, 2))
            {
                if (pts.Count >= 2)
                    g.DrawLines(pen, pts.ToArray());
                foreach (var p in pts)
                {
                    g.FillEllipse(Brushes.Red, p.X - 3, p.Y - 3, 6, 6);
                }
            }

            // border
            g.DrawRectangle(Pens.Gray, plotRect);
        }
    }
}
