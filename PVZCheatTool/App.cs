using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PvzHakTool.Objects;

namespace PvzHakTool
{
    using gweb;
    using Misc;
    // using KeyToString;

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
        // Thank You Tech CBT: https://www.youtube.com/watch?v=nwV3MS6pryY
        public TcpListener server;
        public TcpClient client;
        public NetworkStream stream;
        public bool updateClient = false;
        public IPAddress localhostIP = IPAddress.Parse("127.0.0.1");
        public int PORT = 0;
        public static string Watermark = @"PVZ HakTool V1.0.0c";
        public string folder = "";
        public string file = "";
        public bool checkServer = false;
        /*public Thread resetInputThread;
        public Thread keyChecking;*/
        public Thread hakThread;
        public Thread gameCheckThread;
        public Thread serverThread;
        public Thread clientThread;
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

        public Process console_process;
        public Process pvz_process;

        public bool isPortAvailable(int port)
        {
            // Thank You: https://stackoverflow.com/questions/570098/in-c-how-to-check-if-a-tcp-port-is-available
            List<IPEndPoint> availablePorts = new List<IPEndPoint>();
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            IPEndPoint[] endPointsTcp = properties.GetActiveTcpListeners();
            availablePorts.AddRange(endPointsTcp);

            foreach (IPEndPoint endPoint in availablePorts)
            {
                if (endPoint.Port == port) return false;
            }
            return true;
        }

        public void Start()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Watermark);
            Console.WriteLine("Finding Free Port...");
            do
            {
                Random _random = new Random();
                PORT = _random.Next(0, 65535);
            } while (!isPortAvailable(PORT));
            Console.WriteLine("Using Port " + PORT);
            console_process = null;
            Thread nodeRunThread = new Thread(runNode);
            nodeRunThread.Start();
            Console.WriteLine("Starting Server...");
            server = new TcpListener(localhostIP, PORT);
            server.Start();
            checkServer = true;
            serverThread = new Thread(serverUpdate);
            serverThread.Start();
            Console.WriteLine("Switching To Node Window...");
        }

        /*public void StartBak()
        {
            PORT = 9000;
            do
            {
                Random _random = new Random();
                PORT = _random.Next(0, 65535);
            } while (!isPortAvailable(PORT));
            Console.WriteLine(PORT);
            Console.WriteLine(isPortAvailable(PORT));
            string folderPath = Prompt.createInput("Enter Your PVZ Folder Path (Enter If Current Folder And '..' If It Is The Previous Folder)");
            if (folderPath == "") folderPath = Directory.GetCurrentDirectory(); else folderPath = Path.GetFullPath(folderPath);
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
        }*/

        /*public void afterPromptExe(string fol, string fil)
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
                sendToClient = "Hooking app into PVZ process...";
            }
            else
            {
                sendToClient = "PVZ Process Not Found!\nTerminating Program In 3 Seconds... (It Probably Did Not Launch Yet)";
            }
        }*/

        public bool afterPromptExe(string fol, string fil)
        {
            this.folder = fol;
            this.file = fil;
            Thread pvzRunThread = new Thread(runPVZ);
            pvzRunThread.Start();
            System.Threading.Thread.Sleep(3000);
            Process[] processes = Process.GetProcesses();
            bool pvzExists = false;
            for (int i = 0; i < processes.Length; i++)
            {
                if (this.file == processes[i].ProcessName) pvzExists = true;
            }
            return pvzExists;
        }

        public void runPVZ()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(Path.Combine(this.folder, this.file));
            startInfo.WorkingDirectory = this.folder;
            pvz_process = Process.Start(startInfo);
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
                /*Console.WriteLine("Successfully hooked app into PVZ!");
                Console.WriteLine("Loading Haks...");
                Console.WriteLine("(Might Take 3 Seconds)");*/
                consoleLogClient("Successfully hooked app into PVZ!");
                consoleLogClient("Loading Haks...");
                consoleLogClient("(Wait For 3 Seconds)");
                gameCheckThread = new Thread(checkGame);
                gameCheckThread.Start();
                System.Threading.Thread.Sleep(3000);
                /*keyChecking = new Thread(startKeyCheck);
                keyChecking.Start();*/
                hakThread = new Thread(updateHaks);
                hakThread.Start();
                messageToClient("SetHakList", JsonConvert.SerializeObject(hakList));
                messageToClient("ConsoleUpdate", "1");
                playSound("HakHooked.wav");
            }
            else
            {
                messageToClient("ConsoleUpdate", "0");
                consoleLogClient("WOOPS! Your version of PVZ is unsupported");
                consoleLogClient("This could possibly because it is the GOTY edition or it is not PVZ at all!");
                messageToClient("TerminateProcess", "Terminating app in 3 seconds...");
            }
        }

        /*public void startKeyCheck()
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
        }*/

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
            try
            {
                while (true)
                {
                    if (pvz_process != null && pvz_process.HasExited)
                    {
                        /*if (resetInputThread != null)
                        {
                            resetInputThread.Interrupt();
                            resetInputThread.Abort();
                        }
                        if (keyChecking != null)
                        {
                            keyChecking.Interrupt();
                            keyChecking.Abort();
                        }*/
                        pvz_process.Refresh();
                        if (hakThread != null)
                        {
                            hakThread.Interrupt();
                            hakThread.Abort();
                            hakThread = null;
                        }
                        // Console.ForegroundColor = ConsoleColor.White;
                        messageToClient("ConsoleUpdate", "0");
                        messageToClient("ConsoleClear", "1");
                        consoleLogClient(Watermark);
                        consoleLogClient("");
                        consoleLogClient("Couldn't Find Game!");
                        consoleLogClient("(Maybe It Closed?)");
                        messageToClient("TerminateProcess", "Terminating In 3 Seconds...");
                        if (gameCheckThread != null)
                        {
                            gameCheckThread.Interrupt();
                            gameCheckThread.Abort();
                            gameCheckThread = null;
                        }
                    }
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch { };
        }

        public void runNode()
        {
            ProcessStartInfo launchNode = new ProcessStartInfo(Path.GetFullPath("./PvzHakConsole/node16-win-x86.exe"));
            launchNode.WorkingDirectory = Path.GetFullPath("PvzHakConsole");
            launchNode.Arguments = "console.js -c " + PORT;
            if (System.Environment.OSVersion.Version.Major >= 6)
            {
                launchNode.Verb = "runas";
            }
            console_process = Process.Start(launchNode);
        }

        public void serverUpdate()
        {
            try
            {
                while (checkServer)
                {
                    if (console_process != null && console_process.HasExited)
                    {
                        console_process.Refresh();
                        if (server != null)
                        {
                            terminateProcess();
                        }
                        return;
                    }
                    try
                    {
                        /*TcpClient client = server.AcceptTcpClient();
                        data = null;
                        NetworkStream stream = client.GetStream();

                        int i;

                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            data = Encoding.UTF8.GetString(bytes, 0, i);

                            byte[] b1 = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new messageObject("Initialize", Watermark)));
                            messageObject m2 = new messageObject("PromptExe", "NULL");
                            byte[] b3 = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new messageObject("FolderInvalid", "1")));
                            messageObject m4 = new messageObject("PromptConfPathFnd", "NULL");
                            messageObject m5 = new messageObject("ConsoleLog", "");

                            messageObject jsonData = JsonConvert.DeserializeObject<messageObject>(data);
                            switch (jsonData.id)
                            {
                                case "GetWatermark":
                                    if (int.Parse(jsonData.message) > 0) stream.Write(b1, 0, b1.Length);
                                    break;
                                case "FolderPath":
                                    string folderPath = jsonData.message;
                                    if (folderPath == "") folderPath = Directory.GetCurrentDirectory(); else folderPath = Path.GetFullPath(folderPath);
                                    bool dirExists = Directory.Exists(folderPath);
                                    if (dirExists)
                                    {
                                        folderPath = Path.GetFullPath(folderPath);
                                        m2.message = folderPath;
                                        byte[] b2 = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(m2));
                                        string[] m4Message = new string[2];
                                        m4Message[0] = folderPath;
                                        string[] isPVZ = { "Plants", "Zombies", "PVZ.", "PlVsZo", "PVZNormal", "Plants Vs Zombies", "PlantsVsZombies", "PlantsVsZombiesNormal", "Plants Vs Zombies Normal", "Zombies - Copy" };
                                        string[] folderData = Directory.GetFiles(folderPath);
                                        string[] potentiallyPVZ = new string[0];
                                        for (int j = 0; j < folderData.Length; j++)
                                        {
                                            folderData[j] = Path.GetFileName(folderData[j]);
                                            bool isPotentiallyPVZ = false;
                                            for (int k = 0; k < isPVZ.Length; k++)
                                            {
                                                if (folderData[j].Contains(isPVZ[k]))
                                                {
                                                    isPotentiallyPVZ = true;
                                                }
                                            }
                                            if (!folderData[j].EndsWith(".exe")) isPotentiallyPVZ = false;
                                            if (isPotentiallyPVZ)
                                            {
                                                Array.Resize(ref potentiallyPVZ, potentiallyPVZ.Length + 1);
                                                potentiallyPVZ[potentiallyPVZ.GetUpperBound(0)] = folderData[j];
                                            }
                                        }
                                        Array.Sort(potentiallyPVZ, (x, y) => String.Compare(x, y));
                                        if (potentiallyPVZ.Length > 0)
                                        {
                                            m4Message[1] = potentiallyPVZ[0];
                                            m4.message = JsonConvert.SerializeObject(m4Message);
                                            byte[] b4 = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(m4));
                                            stream.Write(b4, 0, b4.Length);
                                        }
                                        else stream.Write(b2, 0, b2.Length);
                                    }
                                    else
                                    {
                                        stream.Write(b3, 0, b3.Length);
                                    }
                                    break;
                                case "TerminateProcess":
                                    if (int.Parse(jsonData.message) > 0) terminateProcess();
                                    break;
                                case "ExePath":
                                    string arrMessageStr = jsonData.message;
                                    string[] arrMessage = new string[3];
                                    string[] toCheck = JsonConvert.DeserializeObject<string[]>(arrMessageStr);
                                    Console.WriteLine(arrMessageStr);
                                    Console.WriteLine(toCheck[0]);
                                    if (toCheck.Length == 3)
                                    {
                                        arrMessage = toCheck;

                                        string fPath = arrMessage[0];
                                        string exePath = arrMessage[1];
                                        bool forceComplete = int.Parse(arrMessage[2]) > 0;

                                        string combined = Path.Combine(fPath, exePath);
                                        if (File.Exists(combined) || forceComplete)
                                        {
                                            bool pvzExists = afterPromptExe(fPath, exePath);
                                            if (pvzExists) m5.message = "Hooking app into PVZ process..."; else m5.message = "PVZ Process Not Found!\nTerminating Program In 3 Seconds... (It Probably Did Not Launch Yet)";
                                            if (!pvzExists) m5.id = "TerminateProcess";
                                            byte[] b5 = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(m5));
                                            stream.Write(b5, 0, b5.Length);
                                        }
                                        else
                                        {
                                            m2.id = "PromptExeFail";
                                            string[] m2Message = new string[2];
                                            m2Message[0] = fPath;
                                            m2Message[1] = exePath;
                                            Console.WriteLine(exePath);
                                            m2.message = JsonConvert.SerializeObject(m2Message);
                                            byte[] b2 = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(m2));
                                            stream.Write(b2, 0, b2.Length);
                                        }
                                    }
                                    break;
                                default:
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("WARNING: Node Console sent an unrecognized command!");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    break;
                            }
                        }

                        client.Close();
                        // Thank You: https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener
                        */
                        if (client == null)
                        {
                            client = server.AcceptTcpClient();
                            updateClient = true;
                            clientThread = new Thread(clientUpdate);
                            clientThread.Start();
                        }
                    }
                    catch { }
                    finally { server.Stop(); };
                }
            }
            catch { };
        }

        public bool isJson(string inp)
        {
            try
            {
                JsonConvert.DeserializeObject(inp);
                return true;
            }
            catch { return false; };
        }

        public void terminateProcess()
        {
            checkServer = false;
            Console.Clear();
            /*IntPtr hWND = Xtra.GetConsoleWindow();
            Xtra.ShowWindow(hWND, Xtra.SW_SHOW);
            Console.WriteLine("Node Console Cannot Be Found!");
            Console.WriteLine("Terminating In 3 Seconds...");
            if (hakThread != null)
            {
                hakThread.Interrupt();
                hakThread.Abort();
            }
            if (gameCheckThread != null)
            {
                gameCheckThread.Interrupt();
                gameCheckThread.Abort();
            }
            System.Threading.Thread.Sleep(3000);
            Thread startThread = new Thread(Start);
            startThread.Start();*/
            if (hakThread != null)
            {
                hakThread.Interrupt();
                hakThread.Abort();
            }
            if (gameCheckThread != null)
            {
                gameCheckThread.Interrupt();
                gameCheckThread.Abort();
            }
            if (pvz_process != null && !pvz_process.HasExited)
            {
                pvz_process.CloseMainWindow();
                pvz_process.Close();
                pvz_process.Refresh();
            }
            if (server != null) server.Stop();
            if (serverThread != null) serverThread.Abort();
            Environment.Exit(0);
        }

        public void clientUpdate()
        {
            try
            {
                int dataBufferSize = 4096;
                Byte[] bytes = new Byte[dataBufferSize];
                String data = null;

                if (client != null)
                {
                    client.ReceiveBufferSize = dataBufferSize;
                    client.SendBufferSize = dataBufferSize;
                    stream = client.GetStream();
                }

                while (updateClient)
                {
                    if (client != null)
                    {
                        int readLength = stream.Read(bytes, 0, bytes.Length);
                        if (readLength > 0) data = Encoding.UTF8.GetString(bytes, 0, readLength);
                        if (readLength > 0 && isJson(data))
                        {
                            string s1 = JsonConvert.SerializeObject(new messageObject("Initialize", Watermark));
                            messageObject m2 = new messageObject("PromptExe", "NULL");
                            string s3 = JsonConvert.SerializeObject(new messageObject("FolderInvalid", "1"));
                            messageObject m4 = new messageObject("PromptConfPathFnd", "NULL");
                            messageObject m5 = new messageObject("ConsoleLog", "");

                            messageObject jsonData = JsonConvert.DeserializeObject<messageObject>(data);
                            switch (jsonData.id)
                            {
                                case "GetWatermark":
                                    if (int.Parse(jsonData.message) > 0)
                                    {
                                        sendToClient(s1);
                                        IntPtr hWND = Xtra.GetConsoleWindow();
                                        Xtra.ShowWindow(hWND, Xtra.SW_HIDE);
                                    }
                                    break;
                                case "FolderPath":
                                    try
                                    {
                                        string folderPath = jsonData.message;
                                        if (folderPath == "") folderPath = Directory.GetCurrentDirectory(); else folderPath = Path.GetFullPath(folderPath);
                                        bool dirExists = Directory.Exists(folderPath);
                                        if (dirExists)
                                        {
                                            folderPath = Path.GetFullPath(folderPath);
                                            m2.message = folderPath;
                                            string s2 = JsonConvert.SerializeObject(m2);
                                            string[] m4Message = new string[2];
                                            m4Message[0] = folderPath;
                                            string[] isPVZ = { "Plants", "Zombies", "PVZ.", "PlVsZo", "PVZNormal", "Plants Vs Zombies", "PlantsVsZombies", "PlantsVsZombiesNormal", "Plants Vs Zombies Normal", "Zombies - Copy" };
                                            string[] folderData = Directory.GetFiles(folderPath);
                                            string[] potentiallyPVZ = new string[0];
                                            for (int j = 0; j < folderData.Length; j++)
                                            {
                                                folderData[j] = Path.GetFileName(folderData[j]);
                                                bool isPotentiallyPVZ = false;
                                                for (int k = 0; k < isPVZ.Length; k++)
                                                {
                                                    if (folderData[j].Contains(isPVZ[k]))
                                                    {
                                                        isPotentiallyPVZ = true;
                                                    }
                                                }
                                                if (!folderData[j].EndsWith(".exe")) isPotentiallyPVZ = false;
                                                if (isPotentiallyPVZ)
                                                {
                                                    Array.Resize(ref potentiallyPVZ, potentiallyPVZ.Length + 1);
                                                    potentiallyPVZ[potentiallyPVZ.GetUpperBound(0)] = folderData[j];
                                                }
                                            }
                                            Array.Sort(potentiallyPVZ, (x, y) => String.Compare(x, y));
                                            if (potentiallyPVZ.Length > 0)
                                            {
                                                potentiallyPVZ[0] = Path.GetFileNameWithoutExtension(potentiallyPVZ[0]);
                                                m4Message[1] = potentiallyPVZ[0];
                                                m4.message = JsonConvert.SerializeObject(m4Message);
                                                string s4 = JsonConvert.SerializeObject(m4);
                                                sendToClient(s4);
                                            }
                                            else sendToClient(s2);
                                        }
                                        else
                                        {
                                            sendToClient(s3);
                                        }
                                    }
                                    catch { sendToClient(s3); };
                                    break;
                                case "TerminateProcess":
                                    if (int.Parse(jsonData.message) > 0) terminateProcess();
                                    break;
                                case "ExePath":
                                    try
                                    {
                                        string arrMessageStr = jsonData.message;
                                        if (isJson(arrMessageStr))
                                        {
                                            string[] arrMessage = new string[3];
                                            string[] toCheck = JsonConvert.DeserializeObject<string[]>(arrMessageStr);
                                            if (toCheck.Length == arrMessage.Length)
                                            {
                                                arrMessage = toCheck;

                                                string fPath = arrMessage[0];
                                                string exePath = arrMessage[1];
                                                bool forceComplete = int.Parse(arrMessage[2]) > 0;

                                                string combined = Path.Combine(fPath, exePath);
                                                if (File.Exists(combined) || forceComplete)
                                                {
                                                    bool pvzExists = afterPromptExe(fPath, exePath);
                                                    if (pvzExists) m5.message = "Hooking app into PVZ process..."; else m5.message = "PVZ Process Not Found!\nTerminating Program In 3 Seconds... (It Probably Did Not Launch Yet)";
                                                    if (!pvzExists) m5.id = "TerminateProcess";
                                                    string s5 = JsonConvert.SerializeObject(m5);
                                                    sendToClient(s5);
                                                    if (pvzExists) new Thread(initHak).Start();
                                                }
                                                else
                                                {
                                                    m2.id = "PromptExeFail";
                                                    string[] m2Message = new string[2];
                                                    m2Message[0] = fPath;
                                                    m2Message[1] = exePath;
                                                    m2.message = JsonConvert.SerializeObject(m2Message);
                                                    string s2 = JsonConvert.SerializeObject(m2);
                                                    sendToClient(s2);
                                                }
                                            }
                                        }
                                    }
                                    catch { terminateProcess(); };
                                    break;
                                /*case "HakList":
                                    if (isJson(jsonData.message))
                                    {
                                        HakList parsedList = JsonConvert.DeserializeObject<HakList>(jsonData.message);
                                        if (parsedList.Haks != null)
                                        {
                                            bool notNull = parsedList.Haks.Length > 0;
                                            foreach (Hak hak in parsedList.Haks) if (!(hak.HakName != null && hak.Enabled != null && hak.Addresses != null && hak.Values != null)) notNull = false;
                                            if (notNull)
                                            {
                                                Console.WriteLine(parsedList);
                                            }
                                        }
                                    }
                                    break;*/
                                case "UpdateHaks":
                                    try
                                    {
                                        string arrMessageStr = jsonData.message;
                                        if (isJson(arrMessageStr))
                                        {
                                            bool[] arrMessage = new bool[hakList.Haks.Length];
                                            bool[] toCheck = JsonConvert.DeserializeObject<bool[]>(arrMessageStr);
                                            if (toCheck.Length == hakList.Haks.Length)
                                            {
                                                arrMessage = toCheck;

                                                for (int i = 0; i < hakList.Haks.Length; i++)
                                                {
                                                    hakList.Haks[i].Enabled = arrMessage[i];
                                                }
                                            }
                                        }
                                    }
                                    catch { };
                                    break;
                                case "PlayOnOff":
                                    if (int.Parse(jsonData.message) > 0) playSound("HakOn.wav"); else playSound("HakOff.wav");
                                    break;
                                default:
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("WARNING: Node Console sent an unrecognized command!");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    break;
                            }
                        }
                    }
                }
            }
            catch { };
        }

        public void sendToClient(string _data)
        {
            byte[] _dataBytes = Encoding.UTF8.GetBytes(_data);
            if (client != null)
            {
                client.NoDelay = true;
                stream.Write(_dataBytes, 0, _dataBytes.Length);
                System.Threading.Thread.Sleep(100);
            }
        }

        public void consoleLogClient(string log)
        {
            sendToClient(JsonConvert.SerializeObject(new messageObject("ConsoleLog", log)));
        }

        public void messageToClient(string id, string message)
        {
            sendToClient(JsonConvert.SerializeObject(new messageObject(id, message)));
        }
    }
}
