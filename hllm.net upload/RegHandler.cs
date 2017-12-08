using System;
using Microsoft.Win32;
using System.Windows.Forms;
using System.IO;

namespace hllm.net_upload
{
    public static class RegistryHandler
    {
        //function for setting the regvalue
        public static void setValue(string name, string value)
        {
            try
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\HllmUp", name, value);
            }//make shure we don't crash but close
            catch (Exception e)
            {
                //MessageBox.Show("An Error ucorred while trying to writing to the registry!\n" + e.Message);
                
            }
        }
        //function for getting the regvalue
        public static string getValue(string name)
        {
            try
            {
                return (Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\HllmUp", name, "").ToString());
            }
            catch (Exception e)
            {
                //MessageBox.Show("An Error ucorred while trying to read from the registry!\n" + e.Message + "\n" + e.HelpLink);
                //Application.Exit();
                return "";
            }
        }

        public static void DelKey(string name)
        {
            try
            {
                using (RegistryKey explorerKey =
       Registry.CurrentUser.OpenSubKey(name, writable: true))
                {
                    if (explorerKey != null)
                    {
                        explorerKey.DeleteSubKeyTree("FileExts");
                    }
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show("An Error ucorred while trying to read from the registry!\n" + e.Message + "\n" + e.HelpLink);
                //Application.Exit();
            }
        }

        public static void RegisterRC()
        {
            string curPath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Substring(8).Replace('/', '\\');
            string newFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\HllmUp";
            string newPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\HllmUp\HllmUp.exe";

            if (!System.IO.Directory.Exists(newFolder))
            {
                Directory.CreateDirectory(newFolder);
            }

            System.IO.File.Copy(curPath,newPath, true);

            try
            {
                Registry.SetValue(@"HKEY_CLASSES_ROOT\*\shell\Upload\command", null, "\"" + newPath + "\" \"%1\"" );
                Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\shell\Upload\command", null, "\"" + newPath + "\" \"%1\"");
            }//make shure we don't crash but close
            catch (Exception e)
            {
                MessageBox.Show("An Error ucorred while trying to writing to the registry!\n" + e.Message);
            }
        }

        public static void DeregisterRC()
        {
            string Folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\HllmUp";

            Directory.Delete(Folder,true);

            DelKey(@"HKEY_CLASSES_ROOT\*\shell\Upload");
            DelKey(@"HKEY_CLASSES_ROOT\Directory\shell\Upload");

            DelKey(@"HKEY_CURRENT_USER\SOFTWARE\HllmUp");

        }
    }
}
