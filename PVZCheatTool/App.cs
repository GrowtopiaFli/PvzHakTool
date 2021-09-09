using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Media;

namespace PvzHakTool
{
    using gweb;
    using Misc;
    using KeyToString;

    public struct HakList
    {
        public Hak[] Haks;
        public HakList(Hak[] Haks)
        {
            this.Haks = Haks;
        }
    }

    public struct Hak
    {
        public string HakName;
        public bool Enabled;
        public int[] Addresses;
        public Int64[,] Values;
        public Hak(string HakName, bool Enabled, int[] Addresses, Int64[,] Values)
        {
            this.HakName = HakName;
            this.Enabled = Enabled;
            this.Addresses = Addresses;
            this.Values = Values;
        }
    }

    public class App
    {
        public static string Watermark = @"PVZ HakTool V1.0.0b";
        public string folder = "";
        public string file = "";
        public string fname = "";
        public Thread resetInputThread;
        public Thread keyChecking;
        public Thread hakThread;
        public Thread gameCheckThread;
        public string Typed = "";

        public bool invalidCommand = false;

        public string prefix = "tog";

        public const Int64 bytePermission = 1;
        public const Int64 int16Permission = 2;
        public const Int64 int32Permission = 4;
        public const Int64 int64Permission = 8;

        public HakList hakList = new HakList(new Hak[] {
            new Hak(
                "Ignore Sun",
                false,
                    new int[] {
                        0x0041ba72,
                        0x0041ba74,
                        0x0041bac0,
                        0x00427a92,
                        0x00427dfd,
                        0x0042487f
                    },
                    new Int64[,] {
                        { bytePermission, 0x7f, 0x70 },
                        { bytePermission, 0x2b, 0x3b },
                        { bytePermission, 0x9e, 0x91 },
                        { bytePermission, 0x8f, 0x80 },
                        { bytePermission, 0x8f, 0x80 },
                        { bytePermission, 0x74, 0xeb }
                    }
                ),
            new Hak(
                "No Slot Cooldown",
                false,
                    new int[] {
                        0x00487296,
                        0x00488250
                    },
                    new Int64[,] {
                        { bytePermission, 0x7e, 0x70 },
                        { bytePermission, 0x75, 0xeb }
                    }
                ),
            new Hak(
                "Limbo Page",
                false,
                    new int[] {
                        0x0042df5d,
                        0x0042df5e,
                        0x0042df5f
                    },
                    new Int64[,] {
                        { bytePermission, 136, 144 },
                        { bytePermission, 89, 144 },
                        { bytePermission, 84, 144 }
                    }
                ),
            new Hak(
                "Plant Freely",
                false,
                    new int[] {
                        0x0040fe30,
                        0x00438e40,
                        0x0042a2d9
                    },
                    new Int64[,] {
                        { bytePermission, 0x84, 0x81 },
                        { bytePermission, 0x74, 0xeb },
                        { bytePermission, 0x84, 0x8d }
                    }
                ),
            new Hak(
                "One Hit Plants",
                false,
                    new int[] {
                        0x0053130f,
                        0x00531053,
                        0x0053105e,
                        0x00530ca1
                    },
                    new Int64[,] {
                        { int32Permission, 0x20247c2b, 0x90243c2b },
                        { bytePermission, 0x74, 0xeb },
                        { bytePermission, 0x8b, 0x2b },
                        { bytePermission, 0x29, 0x89 }
                    }
                ),
            new Hak(
                "No Plant Cooldown (Cob Cannon, Magnets, Potato Mine, Chomper)",
                false,
                    new int[] {
                        0x0046103b,
                        0x00461e37,
                        0x0045fe54,
                        0x00461565
                    },
                    new Int64[,] {
                        { bytePermission, 0x85, 0x80 },
                        { bytePermission, 0x85, 0x80 },
                        { bytePermission, 0x85, 0x80 },
                        { bytePermission, 0x75, 0x70 }
                    }
                ),
            new Hak(
                "Immediate Plant Explosion (Cherry Bomb, Jalapeno)",
                false,
                    new int[] {
                        0x00463408
                    },
                    new Int64[,] {
                        { bytePermission, 0x75, 0x74 }
                    }
                ),
            new Hak(
                "Attack Superposition",
                false,
                    new int[] {
                        0x00464a97
                    },
                    new Int64[,] {
                        { bytePermission, 0x85, 0x84 }
                    }
                )
        });

        public Process pvz_process;

        public void Start()
        {
            string folderPath = Prompt.createInput("Enter Your PVZ Folder Path (Enter If Current Folder And '..' If It Is The Previous Folder)");
            if (folderPath == "") folderPath = Directory.GetCurrentDirectory();
            bool dirExists = Directory.Exists(folderPath);
            if (dirExists)
            {
                folderPath = Path.GetFullPath(folderPath);
                string[] isPVZ = { "Plants", "Zombies", "PVZ.", "PlVsZo", "PVZNormal", "Plants Vs Zombies", "PlantsVsZombies", "PlantsVsZombiesNormal", "Plants Vs Zombies Normal", "Zombies - Copy" };
                string[] folderData = Directory.GetFiles(folderPath);
                string[] potentiallyPVZ = new string[0];
                for (int i = 0; i < folderData.Length; i++)
                {
                    folderData[i] = Path.GetFileName(folderData[i]);
                    bool isPotentiallyPVZ = false;
                    for (int j = 0; j < isPVZ.Length; j++)
                    {
                        if (folderData[i].Contains(isPVZ[j]))
                        {
                            isPotentiallyPVZ = true;
                        }
                    }
                    if (!folderData[i].EndsWith(".exe")) isPotentiallyPVZ = false;
                    if (isPotentiallyPVZ)
                    {
                        Array.Resize(ref potentiallyPVZ, potentiallyPVZ.Length + 1);
                        potentiallyPVZ[potentiallyPVZ.GetUpperBound(0)] = folderData[i];
                    }
                }
                Array.Sort(potentiallyPVZ, (x, y) => String.Compare(x, y));
                if (potentiallyPVZ.Length > 0)
                {
                    string confirmReturned = Prompt.createInput("Do You Want To Use The Path We Found? (" + potentiallyPVZ[0] + ") (y/N)");
                    if (Prompt.confirm(confirmReturned))
                    {
                        afterPromptExe(folderPath, potentiallyPVZ[0]);
                    }
                    else promptExe(folderPath);
                }
                else promptExe(folderPath);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("The Folder " + folderPath + " Does Not Exist!");
                Start();
            }
        }

        public void promptExe(string fPath)
        {
            string exePath = Prompt.createInput("Enter Your PVZ Exe File");
            string combined = Path.Combine(fPath, exePath);
            if (File.Exists(combined))
            {
                afterPromptExe(fPath, exePath);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("The File " + exePath + " Does Not Exist!");
                promptExe(fPath);
            }
        }

        public void afterPromptExe(string fol, string fil)
        {
            this.folder = fol;
            this.file = fil;
            this.fname = Path.GetFileNameWithoutExtension(this.file);
            launchHak();
        }

        public void launchHak()
        {
            Thread pvzRunThread = new Thread(runPVZ);
            pvzRunThread.Start();
            System.Threading.Thread.Sleep(3000);
            continueHak();
        }

        public void runPVZ()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(Path.Combine(this.folder, this.file));
            startInfo.WorkingDirectory = this.folder;
            this.pvz_process = Process.Start(startInfo);
        }

        public void continueHak()
        {
            Process[] processes = Process.GetProcesses();
            bool pvzExists = false;
            for (int i = 0; i < processes.Length; i++)
            {
                if (this.fname == processes[i].ProcessName) pvzExists = true;
            }
            if (pvzExists)
            {
                Console.WriteLine("Hooking app into PVZ process...");
                initHak();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("PVZ Process Not Found!\nTerminating Program In 3 Seconds... (It Probably Did Not Launch Yet)");
                System.Threading.Thread.Sleep(3000);
                Environment.Exit(0);
            }
        }

        public bool checkPVZ()
        {
            // Checks if it is the GOTY edition or the Old edition
            int oldVerHeader = gwebapi.ReadInt32(this.pvz_process, 0x004140c5);
            if (oldVerHeader == 0x0019b337)
                return true;
            else
                return false;
        }

        public void initHak()
        {
            if (checkPVZ())
            {
                Console.WriteLine("Successfully hooked app into PVZ!");
                Console.WriteLine("Loading Haks...");
                Console.WriteLine("(Might Take 3 Seconds)");
                System.Threading.Thread.Sleep(3000);
                keyChecking = new Thread(startKeyCheck);
                keyChecking.Start();
                hakThread = new Thread(updateHaks);
                hakThread.Start();
                gameCheckThread = new Thread(checkGame);
                gameCheckThread.Start();
                updateConsole();
                playSound("HakHooked.wav");
            }
            else
            {
                Console.WriteLine("WOOPS! Your version of PVZ is unsupported\nThis could possibly because it is the GOTY edition or it is not PVZ at all!\nTerminating app in 3 seconds...");
                System.Threading.Thread.Sleep(3000);
                Environment.Exit(0);
            }
        }

        public void startKeyCheck()
        {
            try
            {
                while (true)
                {
                    for (int i = 0; i < 255; i++)
                    {
                        int keyState = Xtra.GetAsyncKeyState(i);
                        if (keyState == 1 || keyState == -32767)
                        {
                            bool mustReset = false;
                            string toAppend = KeyToString.keyToString(i);
                            if (toAppend.Length == 1)
                            {
                                invalidCommand = false;
                                Typed += toAppend;
                                mustReset = true;
                            }
                            if (toAppend == " Backspace")
                            {
                                invalidCommand = false;
                                if (Typed.Length > 0) Typed = Typed.Remove(Typed.Length - 1, 1);
                                mustReset = true;
                            }
                            if (mustReset)
                            {
                                abortInpThread();
                                resetInputThread = new Thread(resetKeyTimer);
                                resetInputThread.Start();
                            }
                            if (toAppend == " Enter")
                            {
                                abortInpThread();
                                Typed = Typed.TrimStart();
                                if (Typed.StartsWith("tog"))
                                {
                                    try
                                    {
                                        int Parsed = int.Parse(Typed.TrimEnd().Substring(3));
                                        if (Parsed < 1) Parsed = 1;
                                        if (Parsed > hakList.Haks.Length) Parsed = hakList.Haks.Length;
                                        Parsed--;
                                        hakList.Haks[Parsed].Enabled = !hakList.Haks[Parsed].Enabled;

                                        if (hakList.Haks[Parsed].Enabled) playSound("HakOn.wav"); else playSound("HakOff.wav");

                                        // Thanks https://stackoverflow.com/questions/22028688/playing-sounds-on-console-c-sharp
                                    }
                                    catch { invalidCommand = true; };
                                }
                                else invalidCommand = true;
                                updateConsole();
                                Typed = "";
                            }
                        }
                    }
                    // Thread.Sleep(50);
                }
            }
            catch { };
        }

        public void abortInpThread()
        {
            if (resetInputThread != null)
            {
                resetInputThread.Interrupt();
                resetInputThread.Abort();
            }
        }

        public bool pressedKey(int vKey)
        {
            int keyState = Xtra.GetAsyncKeyState(vKey);
            if (keyState == 1 || keyState == -32767)
            {
                return true;
            }
            return false;
        }

        public void resetKeyTimer()
        {
            try
            {
                System.Threading.Thread.Sleep(3000);
                Typed = "";
                updateConsole();
                updateConsole();
            }
            catch { };
        }

        public void updateConsole()
        {
            try
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(Watermark);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                for (int i = 0; i < hakList.Haks.Length; i++)
                {
                    bool isOn = hakList.Haks[i].Enabled;
                    string suffix = isOn ? "On" : "Off";
                    Console.Write((i + 1).ToString() + ") " + hakList.Haks[i].HakName + " ");
                    Console.ForegroundColor = isOn ? ConsoleColor.Green : ConsoleColor.Red;
                    Console.Write(suffix);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.WriteLine("Prefix: " + prefix);
                Console.WriteLine("Input: " + Typed);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                if (invalidCommand) Console.WriteLine("Invalid Command! Use " + prefix + " With The Number Choice Next Time.");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch { };
        }

        public void playSound(string sndPath)
        {
            SoundPlayer snd;
            snd = new SoundPlayer(sndPath);
            snd.Load();
            snd.Play();
        }

        public void updateHaks()
        {
            try
            {
                while (true)
                {
                    for (int i = 0; i < hakList.Haks.Length; i++)
                    {
                        Hak pvzHak = hakList.Haks[i];
                        if (pvzHak.Enabled)
                        {
                            for (int j = 0; j < pvzHak.Addresses.Length; j++)
                            {
                                WriteIntToMem(pvz_process, pvzHak.Addresses[j], pvzHak.Values[j, 2], pvzHak.Values[j, 0]);
                            }
                        }
                        else
                        {
                            for (int j = 0; j < pvzHak.Addresses.Length; j++)
                            {
                                WriteIntToMem(pvz_process, pvzHak.Addresses[j], pvzHak.Values[j, 1], pvzHak.Values[j, 0]);
                            }
                        }
                    }
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch { };
        }

        public void WriteIntToMem(Process process, int Addr, Int64 Val, Int64 Perm)
        {
            if (Perm == bytePermission)
            {
                Byte Val_Byte = (Byte)Val;
                gwebapi.WriteByte(process, Addr, Val_Byte);
            }
            else if (Perm == int16Permission)
            {
                Int16 Val_Int16 = (Int16)Val;
                gwebapi.WriteInt16(process, Addr, Val_Int16);
            }
            else if (Perm == int32Permission)
            {
                Int32 Val_Int32 = (Int32)Val;
                gwebapi.WriteInt32(process, Addr, Val_Int32);
            }
            else if (Perm == int64Permission)
            {
                gwebapi.WriteInt64(process, Addr, Val);
            }
        }

        public void checkGame()
        {
            while (true)
            {
                if (pvz_process.HasExited)
                {
                    if (resetInputThread != null)
                    {
                        resetInputThread.Interrupt();
                        resetInputThread.Abort();
                    }
                    if (keyChecking != null)
                    {
                        keyChecking.Interrupt();
                        keyChecking.Abort();
                    }
                    if (hakThread != null)
                    {
                        hakThread.Interrupt();
                        hakThread.Abort();
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Clear();
                    Console.WriteLine(Watermark);
                    Console.WriteLine();
                    Console.WriteLine("Couldn't Find Game!");
                    Console.WriteLine("(Maybe It Closed?)");
                    Console.WriteLine("Terminating In 5 Seconds...");
                    System.Threading.Thread.Sleep(5000);
                    Environment.Exit(0);
                } 
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
