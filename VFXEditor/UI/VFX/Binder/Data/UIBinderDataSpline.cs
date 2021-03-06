using AVFXLib.Models;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFXEditor.UI.VFX
{
    public class UIBinderDataSpline : UIData {
        public AVFXBinderDataSpline Data;

        public UIBinderDataSpline(AVFXBinderDataSpline data)
        {
            Data = data;
            //==================
            Tabs.Add(new UICurve(data.CarryOverFactor, "Carry Over Factor"));
            Tabs.Add(new UICurve(data.CarryOverFactorRandom, "Carry Over Factor Random"));
        }
    }
}
