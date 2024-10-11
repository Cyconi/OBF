using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TestOBF
{
    public class Class1
    {
        internal static bool wasLaunched;
        internal static Process consoleLogsProcess;
        internal static WebClient webClient;
        internal static void Launch()
        {
            //Console.WriteLine("[Dependencies] Downloading ConsoleLogs.exe...");
            using (WebClient webClient1 = new WebClient()) // why why why whyw hywhywhyhwyhw 
            {
                //webClient1.DownloadFile("https://github.com/Cyconi/EXO-Resources/raw/main/ConsoleLogs.exe", "text");
                Console.WriteLine("Downloaded to: ");
            }
            //Console.ReadKey();
            Console.WriteLine("[Dependencies] ConsoleLogs.exe Downloaded!");
        }
        private static async Task WaitForConnection()
        {
            var client = new TcpClient();
            try
            {
                Console.WriteLine("Connecting...");
                
            }
            catch (Exception ex2)
            {
                Console.WriteLine($"Connection Exception: {ex2}");
            }
        }
        /*internal static void Launch2()
        {
            webClient = new WebClient();
            string text = "ConsoleLogs";
            string text2 = "ConsoleLogs.exe";
            bool flag = Process.GetProcessesByName(text).Length == 0;
            if (flag)
            {
                Console.WriteLine(text + " is not running. Attempting to start...");
                try
                {
                    string text3 = Path.Combine(Environment.CurrentDirectory, "EXO", text2);
                    Console.WriteLine("File path: " + text3);
                    bool flag2 = !File.Exists(text3);
                    if (flag2)
                    {
                        Console.WriteLine("[Dependencies] Downloading ConsoleLogs.exe...");
                        webDownloadFile("https://github.com/Cyconi/EXO-Resources/raw/main/ConsoleLogs.exe", text3);
                        Console.WriteLine("Downloaded to: " + text3);
                        Console.WriteLine("[Dependencies] ConsoleLogs.exe Downloaded!");
                    }
                    consoleLogsProcess = Process.Start(text3);
                    Console.WriteLine(text + " started successfully.");
                    wasLaunched = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to start {text} | Error: {ex.Message}");
                    Console.WriteLine("Stack Trace: " + ex.StackTrace);
                }
                webDispose();
            }
        }
        internal static void OnClose()
        {
            bool flag = wasLaunched && consoleLogsProcess != null;
            if (flag)
                consoleLogsProcess.Close();
        }*/
    }
}
