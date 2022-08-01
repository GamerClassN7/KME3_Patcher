﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using System.IO.Compression;

namespace KME3_Patcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CheckTempFile(System.IO.Path.GetTempPath() + "\\me3_binkw32.zip.zip");
        }

        public static string GetMD5Checksum(string filename)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "");
                }
            }
        }

        public static void  HostsFileAdd(string entry)
        {
            string hostfile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "system32\\drivers\\etc\\hosts");

            CheckTempFile(System.IO.Path.GetTempPath() + "\\hosts");

            File.Copy(hostfile, System.IO.Path.GetTempPath() + "\\hosts");

            string tales = entry;
            string[] lines = File.ReadAllLines(System.IO.Path.GetTempPath() + "\\hosts");

            if (lines.Any(s => s.Contains(entry.Split(' ')[1])))
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(entry.Split(' ')[1]))
                        lines[i] = tales;
                }
                File.WriteAllLines(System.IO.Path.GetTempPath() + "\\hosts", lines);
            }
            else if (!lines.Contains(tales))
            {
                File.AppendAllLines(System.IO.Path.GetTempPath() + "\\hosts", new String[] { tales });
            }

            File.Copy(System.IO.Path.GetTempPath() + "\\hosts", hostfile, true);
        }

        public static bool TestGameServer(string ipAddress = "127.0.0.1")
        {
            return true;
        }

        public static void CheckTempFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.DefaultExt = "MassEffect3.exe";
            openFileDlg.Filter = "Mass Effect 3 (MassEffect3.exe)|MassEffect3.exe";
            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result != true){
                MessageBox.Show("Game nto found");
                return;
            }

            string gameDirectoryPath = System.IO.Path.GetDirectoryName(openFileDlg.FileName);

            WebClient webClient = new WebClient();
            webClient.DownloadFile("https://github.com/Erik-JS/masseffect-binkw32/releases/download/r4/me3_binkw32.zip", System.IO.Path.GetTempPath() + "\\me3_binkw32.zip.zip");

            var checksum = GetMD5Checksum(System.IO.Path.GetTempPath() + "\\me3_binkw32.zip.zip");
            if (checksum != "4A838E04B9BEF86F99F3FC013B65C0BC") {
                MessageBox.Show("checksum doesent match");
                return;
            }

            string zipPath = System.IO.Path.GetTempPath() + "\\me3_binkw32.zip.zip";

            CheckTempFile(gameDirectoryPath + "\\binkw32.dll");
            CheckTempFile(gameDirectoryPath + "\\binkw23.dll");

            ZipFile.ExtractToDirectory(zipPath, gameDirectoryPath);

            HostsFileAdd(ServerAddress.Text + " gosredirector.ea.com");
            HostsFileAdd(ServerAddress.Text + " kme.jacobtread.local");

            MessageBox.Show(checksum);
            ServerAddress.Text = checksum;
        }
    }
}
