using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RhythmGame
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GameForm gameForm = new GameForm();
            Application.Run(gameForm);
        }
    }
}
