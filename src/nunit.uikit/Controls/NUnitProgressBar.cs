// ***********************************************************************
// Copyright (c) 2015 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NUnit.UiKit.Controls
{
    public enum TestProgressBarStatus
    {
        Success = 0,
        Warning = 1,
        Failure = 2
    }
    public enum TestProgressBarTextFormat
    {
        Percent = 0,
        ByTest = 1
    }


    public class NUnitProgressBar : ProgressBar
    {
        public readonly static Color[][] BrushColors =
        {
              new Color[] { Color.FromArgb(32, 205, 32), Color.FromArgb(16, 64, 16) },  // Success
              new Color[] { Color.FromArgb(255, 255, 0), Color.FromArgb(242, 242, 0) }, // Warning
              new Color[] { Color.FromArgb(255, 0, 0), Color.FromArgb(150, 0, 0) }      // Failure
        };

        private Brush _brush;

        private Font _font;
        private FontFamily _fontFamily;
        

        private int _curHeight; // Height changes between constructor and OnPaint. Storing it to detect change so the font size could be recalculated.
        private const string _strFormat = "{0} of {1}";

        public NUnitProgressBar()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            _curHeight = this.Height ;
            _status = TestProgressBarStatus.Success;
            _brush = CreateBrush(_status);
            _fontFamily = FontFamily.GenericSansSerif;
            _font = new Font(_fontFamily, CalculateFontSize());
        }

        #region Properties

        public TestProgressBarTextFormat TextFormat { get; set; }

        private TestProgressBarStatus _status = TestProgressBarStatus.Success;
        public TestProgressBarStatus Status
        {
            get { return _status; }
            set
            {
                if (value != _status)
                {
                    _status = value;

                    if (_brush != null)
                        _brush.Dispose();
                    _brush = CreateBrush(value);
                }
            }
        }

        #endregion

        #region Methods

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rec = this.ClientRectangle;
            rec.Inflate(-2, -2);
            if (ProgressBarRenderer.IsSupported)
                ProgressBarRenderer.DrawHorizontalBar(e.Graphics, rec);
            double percent = ((double)Value / Maximum);
            rec.Inflate(-1, -1);
            rec.Width = (int)(rec.Width * percent);
            e.Graphics.FillRectangle(_brush, rec); //2, 2, rec.Width, rec.Height);
            string strPercent = TextFormat == TestProgressBarTextFormat.ByTest ? String.Format(_strFormat, Value, Maximum) : percent.ToString("P2");
            SizeF strSize;
            if (_curHeight != rec.Height) {
                _curHeight = rec.Height;
                int size = CalculateFontSize();
                Font tmp = new Font(_font.FontFamily, CalculateFontSize());
                strSize = e.Graphics.MeasureString(strPercent, tmp);
                if (strSize.Width <= this.ClientRectangle.Width)
                {
                    _font.Dispose();
                    _font = tmp;
                }else
                {
                    tmp.Dispose();
                }
            }else
            {
                strSize = e.Graphics.MeasureString(strPercent, _font);
            }
            float x = (this.ClientRectangle.Width - strSize.Width) < 0 ? 0: this.ClientRectangle.Width - strSize.Width;
            float y = (this.ClientRectangle.Height - strSize.Height) < 0 ? 0 : this.ClientRectangle.Height - strSize.Height;
            e.Graphics.DrawString(strPercent, _font, new SolidBrush(Color.Black), x / 2, y/ 2);
        }

        private Brush CreateBrush(TestProgressBarStatus status)
        {
            Color[] colors = BrushColors[(int)status];
            return new LinearGradientBrush(
                new Point(0, 0),
                new Point(0, this.ClientSize.Height - 3),
                colors[0],
                colors[1]);
        }

        private int CalculateFontSize() {
            int ascent = _fontFamily.GetCellAscent(FontStyle.Regular) + _fontFamily.GetCellDescent(FontStyle.Regular) + _fontFamily.GetCellDescent(FontStyle.Regular);
            int emHeight = _fontFamily.GetEmHeight(FontStyle.Regular);
            return ((int)(_curHeight*0.9f) * emHeight) / ascent;
        }

        #endregion
    }
}
