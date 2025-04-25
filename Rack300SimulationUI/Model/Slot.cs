using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveUI;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Reactive;
using System.Diagnostics;

namespace Rack300SimulationUI.Model
{
    public class Slot:ReactiveObject
    {
        private readonly Func<bool> _isFillAllowedFunc;
        public Slot(Func<bool> isFillAllowedFunc)
        {
            _isFillAllowedFunc = isFillAllowedFunc;
        }

        //private bool _isFillAllow = false;
        //public bool IsFillAllow
        //{
        //    get => _isFillAllow;
        //    set => this.RaiseAndSetIfChanged(ref _isFillAllow, value);
        //}
        public bool IsFillAllow => _isFillAllowedFunc();

        private bool _isRmvAllow = false;
        public bool IsRmvAllow
        {
            get => _isRmvAllow;
            set => this.RaiseAndSetIfChanged(ref _isRmvAllow, value);
        }

        private int _slotId;
        public int SlotId
        {
            get => _slotId;
            set => this.RaiseAndSetIfChanged(ref _slotId, value);
        }

        private int _ledColorCode = 0;
        public int LedColorCode
        {
            get => _ledColorCode;
            set => this.RaiseAndSetIfChanged(ref _ledColorCode, value);
        }

        private bool _isFilled = false;

        public bool IsFilled
        {
            get => _isFilled;
            set => this.RaiseAndSetIfChanged(ref _isFilled, value);
        }

        private bool _isError = false;
        public bool IsError
        {
            get => _isError;
            set => this.RaiseAndSetIfChanged(ref _isError, value);
        }

        public bool SlotClicked()
        {
            if (IsFilled)
            {

                IsFilled = false;
                if (!IsRmvAllow)
                {
                    LedColorCode = 2;
                    IsError = true;
                }
                else
                {
                    LedColorCode = 0;
                }
                IsRmvAllow = false;
                
                return false;
            }
            else
            {
                IsFilled = true;
                if (!IsFillAllow)
                {
                    LedColorCode = 2;
                    IsError = true;
                }
                return true;
            }

        }
    }
}
