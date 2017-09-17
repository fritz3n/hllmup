﻿using System;
using Microsoft.Win32;
using System.Windows.Forms;

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
                MessageBox.Show("An Error ucorred while trying to writing to the registry!\n" + e.Message);
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
                MessageBox.Show("An Error ucorred while trying to read from the registry!\n" + e.Message + "\n" + e.HelpLink);
                //Application.Exit();
                return "";
            }
        }
    }
}
