using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTunesLib;

namespace iPodSimConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            iTunesApp ita = new iTunesApp();
            SerialPort sp = new SerialPort("COM5", 9600);
            sp.Open();
            string title = "";
            while (true)
            {
                if (title != ita.CurrentTrack.Name)
                {
                    title = ita.CurrentTrack.Name;
                    Console.WriteLine(ita.CurrentTrack.Name);
                    {
                        byte[] ba_title = Encoding.UTF8.GetBytes(title);
                        Array.Resize(ref ba_title, ba_title.Length + 1);
                        ba_title[ba_title.Length - 1] = 0x00;
                        byte[] ba = makeAAPMessage(0x04, new byte[] { 0x00, 0x21 }, ba_title);
                        sp.Write(ba, 0, ba.Length);
                    }
                }
                Console.WriteLine(ita.PlayerPositionMS.ToString());
                {
                    int elapsedtime = ita.PlayerPositionMS;
                    byte[] ba_elapsedtime = new byte[5];
                    ba_elapsedtime[0] = 0x04;
                    ba_elapsedtime[1] = (byte)((elapsedtime >> 24) & 0x0FF);
                    ba_elapsedtime[2] = (byte)((elapsedtime >> 16) & 0x0FF);
                    ba_elapsedtime[3] = (byte)((elapsedtime >> 8) & 0x0FF);
                    ba_elapsedtime[4] = (byte)((elapsedtime >> 0) & 0x0FF);
                    byte[] ba = makeAAPMessage(0x04, new byte[] { 0x00, 0x27 }, ba_elapsedtime);
                    sp.Write(ba, 0, ba.Length);
                }
                Console.WriteLine(ita.PlayerState.ToString());
                {
                    int length = ita.CurrentTrack.Duration * 1000;
                    byte[] ba_length = new byte[4];
                    ba_length[0] = (byte)((length >> 24) & 0x0FF);
                    ba_length[1] = (byte)((length >> 16) & 0x0FF);
                    ba_length[2] = (byte)((length >> 8) & 0x0FF);
                    ba_length[3] = (byte)((length >> 0) & 0x0FF);
                    int time = ita.PlayerPositionMS;
                    byte[] ba_time = new byte[4];
                    ba_time[0] = (byte)((time >> 24) & 0x0FF);
                    ba_time[1] = (byte)((time >> 16) & 0x0FF);
                    ba_time[2] = (byte)((time >> 8) & 0x0FF);
                    ba_time[3] = (byte)((time >> 0) & 0x0FF);
                    byte[] ba_param = new byte[9];
                    Array.Copy(ba_length, 0, ba_param, 0, 4);
                    Array.Copy(ba_time, 0, ba_param, 4, 4);
                    ba_param[ba_param.Length - 1] = (byte)((ita.PlayerState == ITPlayerState.ITPlayerStatePlaying) ? 0x01 : 0x02);
                    byte[] ba = makeAAPMessage(0x04, new byte[] { 0x00, 0x1D }, ba_param);
                    sp.Write(ba, 0, ba.Length);
                }
                System.Threading.Thread.Sleep(500);
            }
            /*
            {
                byte[] ba = { 0xFF, 0x55, 0x0C, 0x04, 0x00, 0x1D, 0x00, 0x03, 0x7E, 0xD8, 0x00, 0x00, 0x22, 0xEB, 0x02, 0x6B };
                sp.Write(ba, 0, ba.Length);
            }
            {
                byte[] ba = { 0xFF, 0x55, 0x08, 0x04, 0x00, 0x27, 0x04, 0x00, 0x03, 0x14, 0x4C, 0x66 };
                sp.Write(ba, 0, ba.Length);
            }
            {
                byte[] ba = { 0xFF, 0x55, 0x0B, 0x04, 0x00, 0x21, 0x61, 0x72, 0x63, 0x61, 0x64, 0x69, 0x61, 0x00, 0x0B };
                sp.Write(ba, 0, ba.Length);
            }
            {
                byte[] ba = { 0xFF, 0x55, 0x0B, 0x04, 0x00, 0x21, 0x53, 0x68, 0x69, 0x6E, 0x65, 0x21, 0x21, 0x00, 0x97 };
                sp.Write(ba, 0, ba.Length);
            }
            {
                byte[] ba = { 0xFF, 0x55, 0x08, 0x04, 0x00, 0x27, 0x04, 0x00, 0x00, 0x01, 0x00, 0xC8 };
                sp.Write(ba, 0, ba.Length);
            }
            {
                byte[] ba = { 0xFF, 0x55, 0x0C, 0x04, 0x00, 0x1D, 0x00, 0x03, 0x7E, 0xD8, 0x00, 0x00, 0x02, 0x44, 0x01, 0x33 };
                sp.Write(ba, 0, ba.Length);
            }
            System.Threading.Thread.Sleep(35000);
            {
                byte[] ba = { 0xFF, 0x55, 0x0C, 0x04, 0x00, 0x1D, 0x00, 0x03, 0x7E, 0xD8, 0x00, 0x00, 0x22, 0xEB, 0x02, 0x6B };
                sp.Write(ba, 0, ba.Length);
            }
            */
            /*
            sp.Close();
            ita.OnPlayerPlayEvent -= Ita_OnPlayerPlayEvent;
            ita = null;
            */
        }

        static byte[] makeAAPMessage(byte md, byte[] cmd, byte[] param)
        {
            int len = 0;
            int sum = 0;
            len = param.Length + 3;
            byte[] msg = new byte[len + 4];
            msg[0] = 0xFF;
            msg[1] = 0x55;
            msg[2] = (byte)len;
            sum += len;
            msg[3] = md;
            sum += md;
            msg[4] = cmd[0];
            sum += cmd[0];
            msg[5] = cmd[1];
            sum += cmd[1];
            for (int i = 0; i < param.Length; i++)
            {
                msg[6 + i] = param[i];
                sum += param[i];
            }
            msg[msg.Length - 1] = (byte)(0x100 - (sum & 0x0FF));
            return msg;
        }
    }
}
