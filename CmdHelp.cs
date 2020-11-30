using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MapNetDrive
{
    public class CmdHelp
    {
        public MapInfo SelectedMapInfo { get; set; }

        public List<MapInfo> MapInfoList => GetMapInfoList();

        /// <summary>
        /// Get MapInfo List from mapinfo.txt
        /// </summary>
        public List<MapInfo> GetMapInfoList()
        {
            var list = new List<MapInfo>();

            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MapNetDrive.txt");
            if (!File.Exists(file))
            {
                return list;
            }

            var txt = File.ReadAllLines(file);

            if (txt.Length <= 0) return list;
            
            foreach (var t in txt)
            {
                if (string.IsNullOrWhiteSpace(t)) continue;

                var map = new MapInfo();

                var line = t.Split('|');

                if (line.Length < 2) continue;
                if (string.IsNullOrWhiteSpace(line[0]) || string.IsNullOrWhiteSpace(line[1])) continue;

                map.Department = line[0].Trim();
                map.NetString = line[1].Trim();

                list.Add(map);
            }

            return list;
        }

        /// <summary>
        /// Get net use cmd string
        /// </summary>
        private string GetNetUseCmdString(string name, string password)
        {
            return $@" {SelectedMapInfo.NetString} /user:{name} {password}";
        }

        /// <summary>
        /// return net use drive letter
        /// </summary>
        private string GetNetUseDriveLetter()
        {
            var index = SelectedMapInfo.NetString.IndexOf(':');
            if (index <= 0) return null;

            var drive = SelectedMapInfo.NetString.Substring(index - 1, 2);
            return drive;
        }


        private string RunCmd(string cmdString)
        {
            if (string.IsNullOrWhiteSpace(cmdString))
            {
                return null;
            }

            var start = new ProcessStartInfo();
            start.FileName = "cmd.exe";
            //start.Arguments = " /c";
            start.UseShellExecute = false;
            start.WorkingDirectory = @"c:\windows\system32";
            start.CreateNoWindow = true;
            start.RedirectStandardError = true;
            start.RedirectStandardInput = true;
            start.RedirectStandardOutput = true;

            using Process p = new Process();
            p.StartInfo = start;
            p.Start();

            p.StandardInput.WriteLine(cmdString);
            p.StandardInput.WriteLine("exit");

            var error = p.StandardError.ReadToEnd();
            //var output = p.StandardOutput.ReadToEnd(); // dont put because will show password

            p.WaitForExit(3000);

            return error;
        }

        /// <summary>
        /// Run Net Use cmd
        /// </summary>
        public string RunNetUseCmd(string name, string password)
        {
            var input = GetNetUseCmdString(name, password);
            var error = RunCmd(input);

            return error;
        }

        /// <summary>
        /// delete net drive letter
        /// </summary>
        public void RunNetUseDeleteCmd()
        {
            var drive = GetNetUseDriveLetter();
            if (drive == null) return;

            //Process.Start("cmd.exe", $" /c net use {drive} /d /y");
            var input = $"net use {drive} /d /y";
            RunCmd(input);
        }

        /// <summary>
        /// open net drive
        /// </summary>
        public void RunOpenDrive()
        {
            var drive = GetNetUseDriveLetter();
            if (drive == null) return;

            //Process.Start("cmd.exe", " /c explorer.exe " + GetNetUseDriveLetter());
            var input = $"explorer.exe {drive}";
            RunCmd(input);
        }
    }
}
