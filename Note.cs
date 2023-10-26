using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RhythmGame
{
    internal class Note : Panel
    {
        
        
        private HoldLine _line;

        private int _xPos;

        private Image _noteImage;
        private Color _color;

        private Keys _keyBind;
        public Keys KeyBind { get { return _keyBind; } }

        string imagesDir = $"{Directory.GetCurrentDirectory()}\\assets\\note";

        public HoldLine Line
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
            switch (noteColor)
            {
                case 'g':
                    _noteImage = Image.FromFile($"{imagesDir}\\green_note_standard.png");
                    _color = Color.Green;
                    _xPos = 65;
                    _keyBind = Keys.A;
                    break;
                case 'r':
                    _noteImage = Image.FromFile($"{imagesDir}\\red_note_standard.png");
                    _color = Color.Red;
                    _xPos = 165;
                    _keyBind = Keys.S;
                    break;
                case 'y':
                    _noteImage = Image.FromFile($"{imagesDir}\\yellow_note_standard.png");
                    _color = Color.Yellow;
                    _xPos = 265;
                    _keyBind = Keys.J;
                    break;
                case 'b':
                    _noteImage = Image.FromFile($"{imagesDir}\\blue_note_standard.png");
                    _color = Color.Blue;
                    _xPos = 365;
                    _keyBind = Keys.K;
                    break;
                case 'o':
                    _noteImage = Image.FromFile($"{imagesDir}\\orange_note_standard.png");
                    _color = Color.Orange;
                    _xPos = 465;
                    _keyBind = Keys.L;
                    break;
            }

            this.Size = new Size(_noteImage.Height, _noteImage.Width);
            this.Location = new Point(_xPos, 0);

            //Note Events
            this.Paint += new PaintEventHandler(Note_Paint);
            this.ControlRemoved += new ControlEventHandler(OnNoteDestroy);
        }
        private void OnNoteDestroy(object sender, ControlEventArgs e)
        {
            _noteImage.Dispose();
            this.Dispose();
        }

        private void Note_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            e.Graphics.DrawImage(_noteImage, 0, 0, 50, 50);
        }
    }

    internal class HoldLine : PictureBox
    {
        public HoldLine(int hldDrtn)
        {
            this.Size = new Size(10, Convert.ToInt32(100 * (hldDrtn * 2 / 200)));
        }
    }
}
