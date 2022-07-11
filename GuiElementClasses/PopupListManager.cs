using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace CadUtility.GuiElementClasses
{
    internal class PopupListManager
    {
        public List<Tuple<InputListBoxPair,Vector2>> popups = new List<Tuple<InputListBoxPair, Vector2>>();
        public PopupListManager()
        {

        }

        public void Draw()
        {
            foreach (var popup in popups)
            {
                popup.Item1.DrawPopupList(popup.Item2);
            }
            popups.Clear();
        }

    }
}
