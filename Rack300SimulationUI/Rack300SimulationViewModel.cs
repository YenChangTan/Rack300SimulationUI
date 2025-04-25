using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rack300SimulationUI.Model;
using System.Reactive;
using ReactiveUI;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using DynamicData;
using System.Windows.Media;
using System.Timers;
using DynamicData.Binding;
using System.Reactive.Linq;

namespace Rack300SimulationUI
{
    public class Rack300SimulationViewModel:ReactiveObject, ITCPControl
    {
        private System.Timers.Timer HeartBeatChecker;
        private System.Timers.Timer IsFillAllowTimer;
        private RmvTimer IsRmvAllowTimer;
        public ObservableCollection<Slot> AllRows { get; set; }
        public ObservableCollection<SideLight> ErrorSideLight { get; set; }
        public ActiveLight IsActiveLight { get; set; }

        private ObservableCollection<int> _isRmvAllowLocation = new ObservableCollection<int>();
        public ObservableCollection<int> IsRmvAllowLocation
        {
            get => _isRmvAllowLocation;
            set
            {
                if(IsRmvAllowLocation != value)
                {
                    _isRmvAllowLocation = value;
                }
            }
        }


        private bool _isFillAllow = false;

        public bool IsFillAllow
        {
            get => _isFillAllow;
            set
            {
                if (_isFillAllow != value)
                {
                    _isFillAllow = value;
                }
            }
        }
        public ReactiveCommand<int, Unit> SlotClickedCommand { get; }

        public Rack300SimulationViewModel()
        {
            AllRows = new ObservableCollection<Slot>();
            SlotClickedCommand = ReactiveCommand.Create<int>(SlotClicked);
            for (int i = 0; i < 300; i++)
            {
                AllRows.Add(new Slot(()=> IsFillAllow)
                {
                    SlotId = i,
                    LedColorCode = 0,
                    IsError = false,
                    

                });
            }
            AllRows
                .ToObservableChangeSet()  // Track changes in AllRows collection
                .AutoRefresh(x => x.IsRmvAllow)  // Automatically refresh when IsRmvAllow changes
                .Subscribe(changes =>
                {
                    
                    IsRmvAllowLocation = new ObservableCollection<int>(AllRows.Where(s => s.IsRmvAllow).Select(s=>s.SlotId));
                });
            ErrorSideLight = new ObservableCollection<SideLight> { new SideLight { ColorCode = 0 }, new SideLight { ColorCode = 0 } };
            IsActiveLight = new ActiveLight { IsActive = false };
            
            //.ToObservableChangeSet()
            //.AutoRefresh(x => x.IsRmvAllow)
            //.ToCollection()
            //.Select(slots => slots.Any(s => s.IsRmvAllow))
            //.ToProperty(this, x => x.IsAnyRmvAllow);


            HeartBeatChecker = new System.Timers.Timer(8000);
            HeartBeatChecker.AutoReset = true;
            HeartBeatChecker.Elapsed += HeartBeatCheckerElapsed;
            HeartBeatChecker.Start();
            IsFillAllowTimer = new System.Timers.Timer(7000);
            IsFillAllowTimer.AutoReset = false;
            IsFillAllowTimer.Elapsed += IsFillAllowTimer_Elapsed;
            IsRmvAllowTimer = new RmvTimer { Interval = 7000 };
            IsRmvAllowTimer.AutoReset = false;
            IsRmvAllowTimer.Elapsed += IsRmvAllowTimer_Elapsed;

        }

        private void IsFillAllowTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            IsFillAllow = false;
        }

        private void IsRmvAllowTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (sender is RmvTimer rmvTimer)
            {
                AllRows[rmvTimer.SlotId].IsRmvAllow = false;
                AllRows[rmvTimer.SlotId].LedColorCode = 0;
            }
        }

        private void HeartBeatCheckerElapsed(object sender, ElapsedEventArgs e)
        {
            IsActiveLight.IsActive = false;
        }



        public void SlotClicked(int SlotId)
        {
            bool ToDisallowFillAll = AllRows[SlotId].SlotClicked();
            if (ToDisallowFillAll)
            {
                IsFillAllow = false;
                IsFillAllowTimer.Stop();
            }
            else
            {
                IsRmvAllowTimer.Stop();
            }
            if (IsRmvAllowLocation.Count != 0)
            {
                foreach (var location in IsRmvAllowLocation)
                {
                    AllRows[location].IsRmvAllow = false;
                    AllRows[location].LedColorCode = 0;
                }
            }
        }


        public byte[] Rd_Err()
        {
            
            byte[] ErrorData = new byte[48];
            for (int i = 0; i <6; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    int SlotBase = i *50 + j*8;
                    int ByteIndex = i* 8 + (j / 2) * 2 - (j % 2) + 1;
                    if (j == 6)
                    {
                        if (AllRows[SlotBase].IsError)
                        {
                            ErrorData[ByteIndex] |= 0b00000001;
                        }
                        if (AllRows[SlotBase + 1].IsError)
                        {
                            ErrorData[ByteIndex] |= 0b00000010;
                        }

                    }
                    else
                    {
                        if (AllRows[SlotBase].IsError)
                        {
                            ErrorData[ByteIndex] |= 0b00000001;
                        }
                        if (AllRows[SlotBase + 1].IsError)
                        {
                            ErrorData[ByteIndex] |= 0b00000010;
                        }
                        if (AllRows[SlotBase + 2].IsError)
                        {
                            ErrorData[ByteIndex] |= 0b00000100;
                        }
                        if (AllRows[SlotBase + 3].IsError)
                        {
                            ErrorData[ByteIndex] |= 0b00001000;
                        }
                        if (AllRows[SlotBase + 4].IsError)
                        {
                            ErrorData[ByteIndex] |= 0b00010000;
                        }
                        if (AllRows[SlotBase + 5].IsError)
                        {
                            ErrorData[ByteIndex] |= 0b00100000;
                        }
                        if (AllRows[SlotBase + 6].IsError)
                        {
                            ErrorData[ByteIndex] |= 0b01000000;
                        }
                        if (AllRows[SlotBase + 7].IsError)
                        {
                            ErrorData[ByteIndex] |= 0b10000000;
                        }
                        
                    }
                }
            }
            return new byte[] { };
        }

        public byte[] Rd_Sen()
        {
            byte[] SensorData = new byte[48];
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    int SlotBase = i * 50 + j * 8;
                    int ByteIndex = i * 8 + (j / 2) * 2 - (j % 2) + 1;
                    bool IsError = false;
                    if (j == 6)
                    {
                        if (AllRows[SlotBase].IsFilled)
                        {
                            SensorData[ByteIndex] |= 0b00000001;
                        }
                        if (AllRows[SlotBase + 1].IsFilled)
                        {
                            SensorData[ByteIndex] |= 0b00000010;
                        }
                        if (!IsError)
                        {
                            for (int k = 0; k < 2; k++)
                            {
                                if (AllRows[SlotBase + k].IsError)
                                {
                                    IsError = true;
                                    SensorData[(i+1) * 8 - 1] |= 0b10000000;
                                    break;
                                };
                            }
                        }
                        
                    }
                    else
                    {
                        if (AllRows[SlotBase].IsFilled)
                        {
                            SensorData[ByteIndex] |= 0b00000001;
                        }
                        if (AllRows[SlotBase + 1].IsFilled  )
                        {
                            SensorData[ByteIndex] |= 0b00000010;
                        }
                        if (AllRows[SlotBase + 2].IsFilled)
                        {
                            SensorData[ByteIndex] |= 0b00000100;
                        }
                        if (AllRows[SlotBase + 3].IsFilled)
                        {
                            SensorData[ByteIndex] |= 0b00001000;
                        }
                        if (AllRows[SlotBase + 4].IsFilled)
                        {
                            SensorData[ByteIndex] |= 0b00010000;
                        }
                        if (AllRows[SlotBase + 5].IsFilled)
                        {
                            SensorData[ByteIndex] |= 0b00100000;
                        }
                        if (AllRows[SlotBase + 6].IsFilled)
                        {
                            SensorData[ByteIndex] |= 0b01000000;
                        }
                        if (AllRows[SlotBase + 7].IsFilled)
                        {
                            SensorData[ByteIndex] |= 0b10000000;
                        }
                        if (!IsError)
                        {
                            for (int k = 0; k < 8; k++)
                            {
                                if (AllRows[SlotBase + k].IsError)
                                {
                                    IsError = true;
                                    SensorData[(i+1) * 8 - 1] |= 0b10000000;
                                    break;
                                };
                            }
                        }
                        

                    }
                }
            }
            return SensorData;
        }

        public void RstErr()
        {
            for (int i = 0; i<300; i++)
            {
                AllRows[i].IsError = false;
                AllRows[i].LedColorCode = 0;
            }
        }

        public void Rly1OnOff(bool On)//BottomRed
        {
            if (On)
            {
                ErrorSideLight[1].ColorCode |= 2;
            }
            else
            {
                ErrorSideLight[1].ColorCode &= ~2;
            }
            
        }

        public void Rly2OnOff(bool On)//BottomGreen
        {
            if (On)
            {
                ErrorSideLight[1].ColorCode |= 1;
            }
            else
            {
                ErrorSideLight[1].ColorCode &= ~1;
            }
        }

        public void Rly3OnOff(bool On)//TopRed
        {
            if (On)
            {
                ErrorSideLight[0].ColorCode |= 2;
            }
            else
            {
                ErrorSideLight[0].ColorCode &= ~2;
            }
        }

        public void Rly4OnOff(bool On)//TopGreen
        {
            if (On)
            {
                ErrorSideLight[0].ColorCode |= 1;
            }
            else
            {
                ErrorSideLight[0].ColorCode &= ~1;
            }
        }

        public void FillOnOff(bool On)
        {
            if (On) 
            {
                ResetFillAllowTimer();
            }
            IsFillAllow = On;
        }

        public void RmvOnOff(bool On)
        {
            if (!On)
            {
                for (int i = 0; i < 300; i++)
                {
                    AllRows[i].IsRmvAllow = On;
                    AllRows[i].LedColorCode = 0;
                }
            }
        }
        public void RmvOnOff(bool On, byte[] DataByte)
        {
            if (On)
            {
                byte[] RemoveLocationByte = new byte[96];
                int copyLength = Math.Min(DataByte.Length, RemoveLocationByte.Length);
                Array.Copy(DataByte, 0, RemoveLocationByte, 0, copyLength);
                int RemoveLocation = -1;
                bool IsMoreThanOne = false;
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        int ByteIndex = i * 8 + j / 2 - j % 2 + 1;
                        int AllRowsBase = i * 50 + j * 8;
                        int m = 8;
                        if (j == 6)
                        {
                            m = 2;
                        }
                        for (int k = 0; k< m; k++)
                        {
                            if (!IsMoreThanOne)
                            {
                                if (((RemoveLocationByte[ByteIndex] >> k) & 0x01) == 1)
                                {
                                    if (RemoveLocation == -1)
                                    {
                                        AllRows[AllRowsBase + k].IsRmvAllow = On;
                                        AllRows[AllRowsBase + k].LedColorCode = 1;
                                        RemoveLocation = AllRowsBase + k;
                                    }
                                    else
                                    {
                                        AllRows[RemoveLocation].IsRmvAllow = false;
                                        AllRows[RemoveLocation].LedColorCode = 0;
                                        AllRows[AllRowsBase + k].IsRmvAllow = false;
                                        AllRows[AllRowsBase + k].LedColorCode = 0;
                                        IsMoreThanOne = true;
                                    }

                                }
                                else
                                {
                                    AllRows[AllRowsBase + k].IsRmvAllow = !On;
                                }
                            }
                            else
                            {
                                AllRows[AllRowsBase + k].IsRmvAllow = false;
                                AllRows[AllRowsBase + k].LedColorCode = 0;
                            }
                        }
                        
                    }
                }
                if (!IsMoreThanOne)
                {

                    //IsRmvAllowTimer.Stop();
                    //IsRmvAllowTimer.SlotId = RemoveLocation;
                    //AllRows[RemoveLocation].IsRmvAllow = true;
                    //IsRmvAllowTimer.Start();
                    ResetRmvAllowTimer(RemoveLocation);
                }
            }
            else
            {
                for (int i = 0;i < 300; i++)
                {
                    AllRows[i].IsRmvAllow = On;

                }
            }
        }

        public void Wr_Led(byte[] DataByte)
        {
            byte[] LedByte = new byte[96];
            int copyLength = Math.Min(DataByte.Length, LedByte.Length);
            Array.Copy(DataByte, 0, LedByte, 0, copyLength);
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    int ByteIndex = i * 16 + j;
                    int RowsIndexBase = i * 50 + j * 4;
                    if (j == 12)
                    {
                        AllRows[RowsIndexBase].LedColorCode = ((LedByte[ByteIndex] >> 1) & 0x01) * 2 + ((LedByte[ByteIndex] >> 5) & 0x01);
                        AllRows[RowsIndexBase + 1].LedColorCode = ((LedByte[ByteIndex] >> 0) & 0x01) * 2 + ((LedByte[ByteIndex] >> 4) & 0x01);

                    }
                    else
                    {
                        AllRows[RowsIndexBase].LedColorCode = ((LedByte[ByteIndex] >> 3) & 0x01)*2 + ((LedByte[ByteIndex] >> 7) & 0x01) ;
                        AllRows[RowsIndexBase + 1].LedColorCode = ((LedByte[ByteIndex] >> 2) & 0x01)*2 + ((LedByte[ByteIndex] >> 6) & 0x01) ;
                        AllRows[RowsIndexBase + 2].LedColorCode = ((LedByte[ByteIndex] >> 1) & 0x01)*2 + ((LedByte[ByteIndex] >> 5) & 0x01) ;
                        AllRows[RowsIndexBase + 3].LedColorCode = ((LedByte[ByteIndex] >> 0) & 0x01)*2 + ((LedByte[ByteIndex] >> 4) & 0x01) ;
                    }
                }
            }
        }

        public void ResetHeartBeat()
        {
            IsActiveLight.IsActive = true;
            HeartBeatChecker.Stop();
            HeartBeatChecker.Start();
        }

        public void ResetFillAllowTimer()
        {
            IsFillAllowTimer.Stop();
            IsFillAllowTimer.Start();
            IsFillAllow = true;
        }
        public void ResetRmvAllowTimer(int SlotId)
        {
            IsRmvAllowTimer.Stop();
            IsRmvAllowTimer.SlotId = SlotId;
            AllRows[SlotId].IsRmvAllow = true;
            IsRmvAllowTimer.Start();

        }
    }
}
