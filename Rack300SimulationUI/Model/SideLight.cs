using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace Rack300SimulationUI.Model
{
    public class SideLight:ReactiveObject
    {
        private int _colorCode;
        public int ColorCode
        {
            get => _colorCode;
            set => this.RaiseAndSetIfChanged(ref _colorCode, value);
        }

    }
}
