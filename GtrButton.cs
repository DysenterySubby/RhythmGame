using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace RhythmGame
{
    internal class GtrButton : PictureBox
    {
        public Image IdlePicture;
        public Image ActivePicture;

        public bool isInvalidated = false;
        public bool isHolding = false;
        public bool isDown = false;

        private string _color;
        public string Color {get { return _color;}}
        public GtrButton(string imageLocation)
        {
            IdlePicture = Image.FromFile(imageLocation);
            ActivePicture = Image.FromFile($"{imageLocation.Remove(imageLocation.LastIndexOf('_'))}_pressed.gif");
            _color = imageLocation.Substring(imageLocation.LastIndexOf('\\') + 1);_color = _color.Remove(_color.IndexOf('_'));

            this.Size = new Size(80, 80);
            this.Image = IdlePicture;
            this.SizeMode = PictureBoxSizeMode.StretchImage;

        }

        public static Keys? GetKey(GtrButton btn)
        {
            Keys? key = null;
            Point location = Point.Empty;
            switch (btn.Color)
            {
                case "green":
                    location = new Point(50, 500);
                    key = Keys.A;
                    break;
                case "red":
                    location = new Point(150, 500);
                    key = Keys.S;
                    break;
                case "yellow":
                    location = new Point(250, 500);
                    key = Keys.J;
                    break;
                case "blue":
                    location = new Point(350, 500);
                    key = Keys.K;
                    break;
                case "orange":
                    location = new Point(450, 500);
                    key =  Keys.L;
                    break;
            }

            btn.Location = location;
            return key;
        }

        public static void KeyDownEvaluate(GtrButton btn)
        {
            //Plays the button animation if pressed
            if (!btn.isHolding)
                btn.Image = btn.ActivePicture;
            //Invalidates key press if the player holds the key down
            if (GameForm.holdElpsd >= 200)
                btn.isDown = false;
            else
                btn.isDown = true;

            //Invalidates the key hold press if the player tries to hold the key down just after a note has just been deactivated
            if (btn.isInvalidated)
                btn.isHolding = false;
            else
                btn.isHolding = true;
        }

        public static void KeyUpEvaluate(GtrButton btn)
        {
            btn.Image = btn.IdlePicture;
            btn.isDown = false;
            btn.isHolding = false;
            btn.isInvalidated = false;
        }
    }
}
