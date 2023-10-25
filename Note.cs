using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RhythmGame
{
    internal class Note : Panel
    {
        private Image _noteImage;
        private LongNote _line;
        private Color _color;
        private int _xPos;


        public GtrButton Button;
        public object Color
        {
            get { return _color; }
            set
            {
                switch (char.Parse(value.ToString()))
                {
                    case 'g':
                        _xPos = 215;
                        _noteImage = Properties.Resources.green_note_standard;
                        _color = System.Drawing.Color.Green;
                        break;
                    case 'r':
                        _xPos = 315;
                        _noteImage = Properties.Resources.red_note_standard;
                        _color = System.Drawing.Color.Red;
                        break;
                }
            }
        }

        public LongNote Line
        {
            get { return _line; }
            set
            {
                _line = value;
                _line.Location = new Point(_xPos + 20, 0);
                _line.BackColor = _color;
            }
        }

        public Note(char noteColor)
        {
            Color = noteColor;
            this.Size = new Size(_noteImage.Height, _noteImage.Width);
            this.Location = new Point(_xPos, 0);

            this.Paint += new PaintEventHandler(Note_Paint);
        }

        private void Note_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            e.Graphics.DrawImage(_noteImage, 0, 0, 50, 50);
        }
    }
}
