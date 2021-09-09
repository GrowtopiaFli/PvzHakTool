using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace gweb
{
    public struct GwebReturnedData
    {
        public Int64 int64;
        public Int32 int32;
        public Int16 int16;
        public Byte uint8;
        public double float64;
        public float float32;
        public GwebReturnedData(Int64 int64, Int32 int32, Int16 int16, Byte uint8, double float64, float float32)
        {
            this.int64 = int64;
            this.int32 = int32;
            this.int16 = int16;
            this.uint8 = uint8;
            this.float64 = float64;
            this.float32 = float32;
        }
    }

    public struct GwebSentData
    {
        public Int64 int64;
        public Int32 int32;
        public Int16 int16;
        public Byte uint8;
        public double float64;
        public float float32;
        public GwebSentData(Int64 int64, Int32 int32, Int16 int16, Byte uint8, double float64, float float32)
        {
            this.int64 = int64;
            this.int32 = int32;
            this.int16 = int16;
            this.uint8 = uint8;
            this.float64 = float64;
            this.float32 = float32;
        }
    }

    public class gwebapi
    {
        public static int processFlag = 0x1f0fff;
        [DllImport("kernel32")]
        public static extern int OpenProcess(int AccessType, int InheritHandle, int ProcessId);
        [DllImport("kernel32", EntryPoint = "WriteProcessMemory")]
        public static extern byte WriteProcessMemoryByte(int Handle, int Address, ref byte Value, int Size, ref int BytesWritten);
        [DllImport("kernel32", EntryPoint = "WriteProcessMemory")]
        public static extern int WriteProcessMemoryInteger(int Handle, int Address, ref int Value, int Size, ref int BytesWritten);
        [DllImport("kernel32", EntryPoint = "WriteProcessMemory")]
        public static extern float WriteProcessMemoryFloat(int Handle, int Address, ref float Value, int Size, ref int BytesWritten);
        [DllImport("kernel32", EntryPoint = "WriteProcessMemory")]
        public static extern double WriteProcessMemoryDouble(int Handle, int Address, ref double Value, int Size, ref int BytesWritten);


        [DllImport("kernel32", EntryPoint = "ReadProcessMemory")]
        public static extern byte ReadProcessMemoryByte(int Handle, int Address, ref byte Value, int Size, ref int BytesRead);
        [DllImport("kernel32", EntryPoint = "ReadProcessMemory")]
        public static extern int ReadProcessMemoryInteger(int Handle, int Address, ref int Value, int Size, ref int BytesRead);
        [DllImport("kernel32", EntryPoint = "ReadProcessMemory")]
        public static extern float ReadProcessMemoryFloat(int Handle, int Address, ref float Value, int Size, ref int BytesRead);
        [DllImport("kernel32", EntryPoint = "ReadProcessMemory")]
        public static extern double ReadProcessMemoryDouble(int Handle, int Address, ref double Value, int Size, ref int BytesRead);
        [DllImport("kernel32")]
        public static extern int CloseHandle(int Handle);

        public static Byte ReadByte(Process process, int Address)
        {
            return ExtReadInt64(process, Address, "Byte").uint8;
        }

        public static Int16 ReadInt16(Process process, int Address)
        {
            return ExtReadInt64(process, Address, "Int16").int16;
        }

        public static Int32 ReadInt32(Process process, int Address)
        {
            return ExtReadInt64(process, Address, "Int32").int32;
        }

        public static Int64 ReadInt64(Process process, int Address)
        {
            return ExtReadInt64(process, Address, "Int64").int64;
        }

        public static float ReadFloat(Process process, int Address)
        {
            return ExtReadDouble(process, Address, "Float").float32;
        }

        public static double ReadDouble(Process process, int Address)
        {
            return ExtReadDouble(process, Address, "Double").float64;
        }

        public static GwebReturnedData ExtReadInt64(Process process, int Address, string IntType)
        {
            GwebReturnedData Value = new GwebReturnedData(0, 0, 0, 0, 0, 0);
            try
            {
                int Bytes = 0;
                int Handle = OpenProcess(processFlag, 0, process.Id);
                if (Handle != 0)
                {
                    switch (IntType)
                    {
                        case "Byte":
                            byte SubValue8 = 0;
                            ReadProcessMemoryByte(Handle, Address, ref SubValue8, 1, ref Bytes);
                            Value.uint8 = SubValue8;
                            break;
                        case "Int16":
                            int Int32_Value = ReadInt32(process, Address);
                            byte[] Int32_Value_Bytes = BitConverter.GetBytes(Int32_Value);
                            Int16 SubValue16 = BitConverter.ToInt16(Int32_Value_Bytes, 0);
                            Value.int16 = SubValue16;
                            break;
                        case "Int32":
                            Int32 SubValue32 = 0;
                            ReadProcessMemoryInteger(Handle, Address, ref SubValue32, 4, ref Bytes);
                            Value.int32 = SubValue32;
                            break;
                        case "Int64":
                            byte[] Bytes8 = ReadByteArray(process, Address, 8);
                            Int64 SubValue64 = BitConverter.ToInt64(Bytes8, 0);
                            Value.int64 = SubValue64;
                            break;
                    }
                    CloseHandle(Handle);
                }
            } catch { };
            return Value;
        }

        public static GwebReturnedData ExtReadDouble(Process process, int Address, string DecimalType)
        {
            GwebReturnedData Value = new GwebReturnedData(0, 0, 0, 0, 0, 0);
            try
            {
                int Bytes = 0;
                int Handle = OpenProcess(processFlag, 0, process.Id);
                if (Handle != 0)
                {
                    switch (DecimalType)
                    {
                        case "Float":
                            float Float_Value = 0;
                            ReadProcessMemoryFloat((int)Handle, Address, ref Float_Value, 4, ref Bytes);
                            Value.float32 = Float_Value;
                            break;
                        case "Double":
                            double Double_Value = 0;
                            ReadProcessMemoryDouble((int)Handle, Address, ref Double_Value, 8, ref Bytes);
                            Value.float64 = Double_Value;
                            break;
                    }
                    CloseHandle(Handle);
                }
            }
            catch { };
            return Value;
        }

        public static void WriteByte(Process process, int Address, Byte value)
        {
            ExtWriteInt64(process, Address, "Byte", new GwebSentData(0, 0, 0, value, 0, 0));
        }

        public static void WriteInt16(Process process, int Address, Int16 value)
        {
            ExtWriteInt64(process, Address, "Int16", new GwebSentData(0, 0, value, 0, 0, 0));
        }

        public static void WriteInt32(Process process, int Address, Int32 value)
        {
            ExtWriteInt64(process, Address, "Int32", new GwebSentData(0, value, 0, 0, 0, 0));
        }

        public static void WriteInt64(Process process, int Address, Int64 value)
        {
            ExtWriteInt64(process, Address, "Int64", new GwebSentData(value, 0, 0, 0, 0, 0));
        }

        public static void WriteFloat(Process process, int Address, float value)
        {
            ExtWriteDouble(process, Address, "Float", new GwebSentData(0, 0, 0, 0, 0, value));
        }

        public static void WriteDouble(Process process, int Address, double value)
        {
            ExtWriteDouble(process, Address, "Double", new GwebSentData(0, 0, 0, 0, value, 0));
        }

        public static void ExtWriteInt64(Process process, int Address, string IntType, GwebSentData sent)
        {
            try
            {
                int Bytes = 0;
                int Handle = OpenProcess(processFlag, 0, process.Id);
                if (Handle != 0)
                {
                    switch (IntType)
                    {
                        case "Byte":
                            WriteProcessMemoryByte(Handle, Address, ref sent.uint8, 1, ref Bytes);
                            break;
                        case "Int16":
                            byte[] uint8Data = new byte[2];
                            uint8Data[0] = (byte)sent.int16;
                            uint8Data[1] = (byte)(sent.int16 >> 8);
                            GwebSentData toSend1 = sent;
                            GwebSentData toSend2 = sent;
                            toSend1.uint8 = uint8Data[0];
                            toSend2.uint8 = uint8Data[1];
                            ExtWriteInt64(process, Address, "Byte", toSend1);
                            ExtWriteInt64(process, Address + 1, "Byte", toSend2);
                            break;
                        case "Int32":
                            WriteProcessMemoryInteger(Handle, Address, ref sent.int32,4, ref Bytes);
                            break;
                        case "Int64":
                            byte[] Bytes8 = BitConverter.GetBytes(sent.int64);
                            WriteByteArray(process, Address, Bytes8);
                            break;
                    }
                    CloseHandle(Handle);
                }
            }
            catch { };
        }

        public static void ExtWriteDouble(Process process, int Address, string DecimalType, GwebSentData sent)
        {
            try
            {
                int Bytes = 0;
                int Handle = OpenProcess(processFlag, 0, process.Id);
                if (Handle != 0)
                {
                    switch (DecimalType)
                    {
                        case "Float":
                            WriteProcessMemoryFloat(Handle, Address, ref sent.float32, 4, ref Bytes);
                            break;
                        case "Double":
                            ReadProcessMemoryDouble(Handle, Address, ref sent.float64, 8, ref Bytes);
                            break;
                    }
                    CloseHandle(Handle);
                }
            }
            catch { };
        }

        public static byte[] ReadByteArray(Process process, int Address, int count)
        {
            if (count > 0)
            {
                try
                {
                    byte[] value = new byte[count];
                    for (int i = 0; i < count; i++)
                    {
                        value[i] = ReadByte(process, Address + i);
                    }
                    return value;
                }
                catch { };
            }
            return null;
        }

        public static void WriteByteArray(Process process, int address, byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                WriteByte(process, address + i, bytes[i]);
            }
        }

        public static string ReadStringUntilNULL(Process process, int Address)
        {
            string value = "";
            bool endOfString = false;
            int counter = 0;
            while (!endOfString)
            {
                if (ReadByte(process, Address + counter) > (byte)0)
                {
                    value += (char)ReadByte(process, Address + counter);
                }
                else
                {
                    return value;
                }
                counter++;
            }
            return value;
        }

        public static void WriteString(Process process, int Address, string value)
        {
            if (value != null)
            {
                int counter = 0;
                foreach (char chr in value.ToCharArray())
                {
                    WriteByte(process, Address + counter, (byte)chr);
                    counter++;
                }
            }
        }

        public static void WriteNOPs(Process process, int address, int count)
        {
            for (int i = 0; i < count; i++)
            {
                WriteByte(process, address + i, 0x90);
            }
        }
    }
}
