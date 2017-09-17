﻿using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Imaging;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;

namespace hllm.net_upload
{
    class Program
    {
        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(System.IntPtr hWnd, int cmdShow);



        [STAThread]
        static void Main()
        {
        string postRequest(string url, NameValueCollection dta)
            {
                string result = "";
                using (WebClient client = new WebClient())
                {

                    byte[] response = client.UploadValues(url, dta);

                    result = System.Text.Encoding.UTF8.GetString(response);
                }
                return result;
            }



            string login()
            {
                Console.WriteLine("Username:");
                string name = Console.ReadLine();
                Console.WriteLine("Password:");
                string pass = null;
                while (true)
                {
                    var key = System.Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                        break;
                    pass += key.KeyChar;
                }

                Console.WriteLine("Checking...");

                string result = postRequest("https://hllm.ddns.net/php/upload/login", new NameValueCollection()
                        {
                {"Username", name },
                {"Password", pass }
                        });
                if (result != "false")
                {
                    return result;
                }
                else
                {
                    writeE("Wrong");
                    return login();
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            String docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            String[] arguments = Environment.GetCommandLineArgs();

            bool commandLine = false;

            // Console.Beep();
            //MessageBox.Show(Environment.CommandLine);

            string[] Filedata;
            bool error = false;
            bool wait = false;



            

            if (String.IsNullOrEmpty(RegistryHandler.getValue("uid")) | String.IsNullOrEmpty(RegistryHandler.getValue("utk")))
            {
                Console.WriteLine("It seems like this is you first time");
                Console.WriteLine("Would you care to give me your data?");
                string data = login();
                //Console.WriteLine("Welcome");
                string[] loginArray = data.Split(',');
                RegistryHandler.setValue("uid", loginArray[0]);
                RegistryHandler.setValue("utk", loginArray[1]);
                Filedata = loginArray;
                //Console.ReadLine();
            }
            else
            {
                string uid = RegistryHandler.getValue("uid");
                string utk = RegistryHandler.getValue("utk");

                Filedata = new string[] {uid,utk};
            }


            string username = postRequest("https://hllm.ddns.net/php/upload/confirm", new NameValueCollection()
                        {
                {"uid", Filedata[0] },
                {"utk", Filedata[1] }
                        });

            if (username == "false")
            {
                writeE("There was a problem, please login again");

                string data = login();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("OK");
                Console.ForegroundColor = ConsoleColor.White;
                string[] loginArray = data.Split(',');
                RegistryHandler.setValue("uid", loginArray[0]);
                RegistryHandler.setValue("utk", loginArray[1]);
                Filedata = loginArray;
                
                username = postRequest("https://hllm.ddns.net/php/upload/confirm", new NameValueCollection()
                        {
                {"uid", Filedata[0] },
                {"utk", Filedata[1] }
                        });
            }

            Console.WriteLine("Welcome, " + username + "!");

            if (arguments.Length > 1)
            {
                switch (arguments[1])
                {
                    case "logout":
                        RegistryHandler.setValue("uid", "");
                        RegistryHandler.setValue("utk", "");
                        Console.WriteLine("Please log back in");
                        string data = login();
                        //Console.WriteLine("Welcome");
                        string[] loginArray = data.Split(',');
                        RegistryHandler.setValue("uid", loginArray[0]);
                        RegistryHandler.setValue("utk", loginArray[1]);
                        Filedata = loginArray;
                        break;

                    case "screenshot":

                        Process proc = Process.Start("snippingtool", "/clip");

                        proc.WaitForExit();

                        if (Clipboard.ContainsImage())
                        {
                            string path = Path.GetTempPath() + "screenshot_" + DateTime.Now.ToString("dd-MM-yy_HH,mm,ss") + ".png";
                            Image img = Clipboard.GetImage();
                            img.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                            upload(path, "screenshots/", true);
                            File.Delete(path);
                            System.Media.SystemSounds.Hand.Play();
                            Environment.Exit(0);
                        }
                        else
                        {
                            writeE("Theres no picture!");
                        }

                        break;

                    case "fullscreenshot":
                        
                        
                        string ImgPath = Path.GetTempPath() + "screenshot_" + DateTime.Now.ToString("dd-MM-yy_HH,mm,ss") + ".png";
                        Bitmap image = screenshot();
                        image.Save(ImgPath, System.Drawing.Imaging.ImageFormat.Png);
                        upload(ImgPath,"screenshots/", true);
                        File.Delete(ImgPath);
                        System.Media.SystemSounds.Hand.Play();
                        Environment.Exit(0);
                        
                        break;

                    default:
                        commandLine = true;
                        break;

                }
            }

            if (!commandLine)
            {
                if (Clipboard.ContainsFileDropList())
                {
                    StringCollection List = Clipboard.GetFileDropList();

                    foreach (string path in List)
                    {
                        upload(path);
                    }

                }
                else
                {
                    error = true;
                    wait = true;
                    writeE("No copied files!");
                    //Console.ReadKey();
                    //System.Threading.Thread.Sleep(2000);
                }
            }
            else
            {
                for (int i = 1; i < arguments.Length; i++)
                {

                    string path = arguments[i];
                    upload(path);

                }
            }

            if (!error)
            {
                System.Media.SystemSounds.Hand.Play();
            }

            if (wait) {
                //Console.WriteLine("Press the 'any' key to continue");
                //Console.ReadKey(true);
                commandlineMode();
            }

            //Console.ReadLine();

            void upload(string path, string subdir = "", bool getToken = false)
            {
                if (File.Exists(path))
                {

                    string dataStr = Convert.ToBase64String(File.ReadAllBytes(path));
                    string filename = Path.GetFileName(path);

                    Console.WriteLine("Uploading " + filename + (subdir != "" ? " under: " + subdir : ""));

                    string response = postRequest("https://hllm.ddns.net/php/upload/upload", new NameValueCollection()
                            {
                            {"uid", Filedata[0] },
                            {"utk", Filedata[1] },
                            {"data", dataStr },
                            {"name", filename },
                            {"dir", subdir }
                            });
                    if (response != "false")
                    {
                        error = false;
                        if (subdir == "" || getToken)
                        {
                            Clipboard.SetText(response);
                        }
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("OK");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        error = true;
                        wait = true;
                        writeE("ERROR UPLOADING");
                        //Console.ReadKey();
                        //System.Threading.Thread.Sleep(2000);
                    }
                }else if (Directory.Exists(path))
                {
                    

                    bool token = false;
                    if(subdir == "" || getToken)
                    {
                        token = true;
                    }

                    Console.WriteLine("Encountered /" + new DirectoryInfo(path).Name + " ...");
                    subdir = subdir + new DirectoryInfo(path).Name + "/";
                    string[] paths = Directory.GetFileSystemEntries(path);
                    foreach (string newpath in paths)
                    {
                        upload(newpath, subdir);
                    }

                    if (token)
                    {
                        string response = postRequest("https://hllm.ddns.net/php/upload/token", new NameValueCollection()
                        {
                        {"uid", Filedata[0] },
                        {"utk", Filedata[1] },
                        {"name", new DirectoryInfo(path).Name },
                        {"dir", "" }
                        });

                        Clipboard.SetText(response);
                    }
                        
                    
                }
            }

            void write(string Str)
            {
                Console.WriteLine(Str);
            }

            void writeE(string Str)
            {
                toFront();
                System.Media.SystemSounds.Asterisk.Play();
                Console.ForegroundColor = ConsoleColor.Yellow;
                write(Str);
                Console.ForegroundColor = ConsoleColor.White;
            }

            void commandlineMode()
            {
                Console.WriteLine("Command line mode!\nuse exit to close");
                while (true)
                {
                    Console.Write(">");
                    string raw = Console.ReadLine();
                    int x = 2;
                    string[] rawArray = raw.Split(new char[] { ' ' }, x);
                    string command = rawArray[0];

                    List<string> args = new List<string>();

                    if (rawArray.Length > 1)
                    {

                        string pattern = "[\"'](\\S*?)[\"']|([^ \"'\\s]+)";

                        Regex rgx = new Regex(pattern);
                        int[] groupNumbers = rgx.GetGroupNumbers();

                        Match m = rgx.Match(rawArray[1]);

                        while (m.Success)
                        {
                            for (int i = 1; i <= 2; i++)
                            {
                                Group g = m.Groups[i];
                                CaptureCollection cc = g.Captures;
                                if (cc.Count > 0)
                                {
                                    args.Add(cc[0].Value);
                                }
                            }
                            m = m.NextMatch();
                        }
                    }

                    switch (command)
                    {
                        case "exit":
                            write("Bye!");
                            System.Threading.Thread.Sleep(500);
                            Environment.Exit(0);
                            break;

                        case "upload":
                            if(args.Count == 0)
                            {
                                OpenFileDialog opf = new OpenFileDialog();
                                DialogResult result =  opf.ShowDialog();

                                if (result == DialogResult.OK)
                                {
                                    upload(opf.FileName);
                                }
                                else
                                {
                                    writeE("There was an problem with:\n" + opf.FileName);
                                }
                            }
                            else
                            {
                                foreach(string path in args)
                                {
                                    upload(path);
                                }
                            }
                            break;

                        case "logout":
                            RegistryHandler.setValue("uid", "");
                            RegistryHandler.setValue("utk", "");
                            Console.WriteLine("Please log back in");
                            string data = login();
                            //Console.WriteLine("Welcome");
                            string[] loginArray = data.Split(',');
                            RegistryHandler.setValue("uid", loginArray[0]);
                            RegistryHandler.setValue("utk", loginArray[1]);
                            Filedata = loginArray;
                            break;

                        case "screenshot":

                            Process proc = Process.Start("snippingtool", "/clip");

                            proc.WaitForExit();

                            if (Clipboard.ContainsImage())
                            {
                                string path = Path.GetTempPath() + "screenshot_" + DateTime.Now.ToString("dd-MM-yy_HH,mm,ss") + ".png";
                                Image img = Clipboard.GetImage();
                                img.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                                upload(path, "screenshots/", true);
                                File.Delete(path);
                            }
                            else
                            {
                                writeE("Theres no picture!");
                            }
                            break;

                        case "fullscreenshot":

                            string ImgPath = Path.GetTempPath() + "screenshot_" + DateTime.Now.ToString("dd-MM-yy_HH,mm,ss") + ".png";
                            Bitmap image = screenshot();
                            image.Save(ImgPath, System.Drawing.Imaging.ImageFormat.Png);
                            upload(ImgPath, "screenshots/", true);
                            File.Delete(ImgPath);

                            break;

                        default:
                            writeE("Unrecognized command: " + command);
                            break;
                    }
                }
            }

            Bitmap screenshot()
            {//Create a new bitmap.

                int screenLeft = SystemInformation.VirtualScreen.Left;
                int screenTop = SystemInformation.VirtualScreen.Top;
                int screenWidth = SystemInformation.VirtualScreen.Width;
                int screenHeight = SystemInformation.VirtualScreen.Height;

                var bmpScreenshot = new Bitmap(screenWidth,
                                               screenHeight,
                                               PixelFormat.Format32bppArgb);

                // Create a graphics object from the bitmap.
                var gfxScreenshot = Graphics.FromImage(bmpScreenshot);

                // Take the screenshot from the upper left corner to the right bottom corner.
                gfxScreenshot.CopyFromScreen(screenLeft,
                                            screenTop,
                                            0,
                                            0,
                                            bmpScreenshot.Size,
                                            CopyPixelOperation.SourceCopy);
                return bmpScreenshot;
            }

            void toFront()
            {
                Process p = Process.GetCurrentProcess();
                ShowWindow(p.MainWindowHandle, 9);
            }

        }
    }
}

/*    error = true;
                    System.Media.SystemSounds.Asterisk.Play();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("There was an ERROR with:\n" + path);
                    Console.ForegroundColor = ConsoleColor.White;
                    //Console.ReadKey();
                    //System.Threading.Thread.Sleep(2000);
                    return;
*/