using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace hllm.net_upload
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
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
                    System.Media.SystemSounds.Asterisk.Play();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Wrong");
                    Console.ForegroundColor = ConsoleColor.White;
                    return login();
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            String docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            String[] arguments = Environment.GetCommandLineArgs();

            bool commandLine = false;

            // Console.Beep();
            //MessageBox.Show(Environment.CommandLine);

            bool error = false;
            bool wait = false;
            bool zipit = false;

            if (arguments.Length > 1)
            {
                if (arguments[1] == "logout")
                {
                    if (File.Exists(docs + @"\hlmup\data.txt"))
                    {
                        File.Delete(docs + @"\hlmup\data.txt");
                        Console.WriteLine("Logged out!");
                    }
                }
                else
                {
                    commandLine = true;
                }
            }

            if (!File.Exists(docs + @"\hlmup\data.txt"))
            {
                Directory.CreateDirectory(docs + @"\hlmup");
                Console.WriteLine("It seems like this is you first time");
                Console.WriteLine("Would you care to give me your data?");
                string data = login();
                    //Console.WriteLine("Welcome");
                    File.WriteAllText(docs + @"\hlmup\data.txt", data);
                //Console.ReadLine();
            }

            string[] Filedata = File.ReadAllLines(docs + @"\hlmup\data.txt")[0].Split(',');

            if(Filedata.Length != 2)
            {
                System.Media.SystemSounds.Asterisk.Play();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was an ERROR");
                Console.WriteLine("Pleas log in again");
                Console.ForegroundColor = ConsoleColor.White;
                string data = login();
                //Console.WriteLine("Welcome");
                File.WriteAllText(docs + @"\hlmup\data.txt", data);
                //Console.ReadLine();
            }

            string username = postRequest("https://hllm.ddns.net/php/upload/confirm", new NameValueCollection()
                        {
                {"uid", Filedata[0] },
                {"utk", Filedata[1] }
                        });

            if (username == "false")
            {
                System.Media.SystemSounds.Asterisk.Play();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was a problem, please login again");
                Console.ForegroundColor = ConsoleColor.White;

                string data = login();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("OK");
                Console.ForegroundColor = ConsoleColor.White;
                File.WriteAllText(docs + @"\hlmup\data.txt", data);

                Filedata = File.ReadAllLines(docs + @"\hlmup\data.txt")[0].Split(',');
                username = postRequest("https://hllm.ddns.net/php/upload/confirm", new NameValueCollection()
                        {
                {"uid", Filedata[0] },
                {"utk", Filedata[1] }
                        });
            }

            Console.WriteLine("Welcome, " + username + "!");
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
                    System.Media.SystemSounds.Asterisk.Play();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No copied files!");
                    Console.ForegroundColor = ConsoleColor.White;
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
                Console.WriteLine("Press the 'any' key to continue");
                Console.ReadKey(true);
            }

            //Console.ReadLine();

            void upload(string path, string subdir = "")
            {
                if (File.Exists(path))
                {

                    string dataStr = Convert.ToBase64String(File.ReadAllBytes(path));
                    string filename = Path.GetFileName(path);

                    Console.WriteLine("Uploading " + filename + " under: " + subdir);

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
                        if (subdir == "")
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
                        System.Media.SystemSounds.Asterisk.Play();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("ERROR UPLOADING");
                        Console.ForegroundColor = ConsoleColor.White;
                        //Console.ReadKey();
                        //System.Threading.Thread.Sleep(2000);
                    }
                }else if (Directory.Exists(path))
                {
                    

                    bool token = false;
                    if(subdir == "")
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