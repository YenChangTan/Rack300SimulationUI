using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rack300SimulationUI
{
    public interface ITCPControl
    {
        byte[] Rd_Sen();
        byte[] Rd_Err();
        void RstErr();

        void FillOnOff(bool On);
        void RmvOnOff(bool On);
        void RmvOnOff(bool On, byte[] RemoveLocationByte);
        void Rly4OnOff(bool On); //Top Green On Off
        void Rly3OnOff(bool On); //Top Red On Off
        void Rly2OnOff(bool On); //Bottom Green On Off
        void Rly1OnOff(bool On); //Bottom Red On Off
        void Wr_Led(byte[] LedByte);
        void ResetHeartBeat();
        void ResetFillAllowTimer();
        void ResetRmvAllowTimer(int SlotId);
    }
}
