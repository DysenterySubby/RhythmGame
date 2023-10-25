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
        public bool isHolding = false;
        public bool isDown = false;
        public string color;
        public GtrButton(string imageLocation)
        {
            IdlePicture = Image.FromFile(imageLocation);
            ActivePicture = Image.FromFile($"{imageLocation.Remove(imageLocation.LastIndexOf('_'))}_pressed.gif");
            color = imageLocation.Substring(imageLocation.LastIndexOf('\\') + 1); color = color.Remove(color.IndexOf('_'));
            Console.WriteLine(color);

            this.Size = new Size(80, 80);
            this.Image = IdlePicture;
            this.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        public static Keys? GetKey(GtrButton btn)
        {
            switch (btn.color)
            {
                case "green":
                    btn.Location = new Point(50, 500);
                    return Keys.A;
                case "red":
                    btn.Location = new Point(150, 500);
                    return Keys.S;
                case "yellow":
                    btn.Location = new Point(250, 500);
                    return Keys.J;
                case "blue":
                    btn.Location = new Point(350, 500);
                    return Keys.K;
                case "orange":
                    btn.Location = new Point(450, 500);
                    return Keys.L;
            }
            return null;
        }

        public static void KeyDownEvaluate(GtrButton btn, long holdElpsd)
        {
            if (!btn.isHolding)
                btn.Image = btn.ActivePicture;
            if (holdElpsd >= 100)
                btn.isDown = false;
            else
                btn.isDown = true;
            btn.isHolding = true;
        }

        public static void KeyUpEvaluate(GtrButton btn)
        {
            btn.Image = btn.IdlePicture;
            btn.isDown = false;
            btn.isHolding = false;
        }
    }
}
