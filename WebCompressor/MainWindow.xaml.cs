using System;
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
using System.IO;
using System.Diagnostics;


namespace WebCompressor
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


        private String BrowseJARFile()
        {
            String filepath = "";
            Microsoft.Win32.OpenFileDialog od = new Microsoft.Win32.OpenFileDialog();
            
            od.DefaultExt = ".jar";
            od.Filter = "Jar Files (*.jar)|*.jar";

            Nullable<bool> result = od.ShowDialog();

            if (result == true)
            {
                filepath = od.FileName;
            }

            return filepath;
        }

        private String BrowseFolder(String path = "")
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = path;
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();


            return dialog.SelectedPath.ToString();
        }

        private void btn_htmc_browse_Click(object sender, RoutedEventArgs e)
        {
            tb_htmlc.Text = BrowseJARFile();
        }

        private void btn_yui_browse_Click(object sender, RoutedEventArgs e)
        {
            tb_yui.Text = BrowseJARFile();
        }

        private void btn_source_browse_Click(object sender, RoutedEventArgs e)
        {
            tb_source.Text = BrowseFolder(tb_source.Text);
        }

        private void btn_target_browse_Click(object sender, RoutedEventArgs e)
        {
            tb_target.Text = BrowseFolder(tb_target.Text);
        }

        public void ExecuteCommand(string command)
        {

            int ExitCode;
            ProcessStartInfo ProcessInfo;
            Process Process;

            ProcessInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            ProcessInfo.CreateNoWindow = true;
            ProcessInfo.UseShellExecute = false;

            Process = Process.Start(ProcessInfo);
            Process.WaitForExit();

            ExitCode = Process.ExitCode;
            Process.Close();

            //MessageBox.Show("ExitCode: " + ExitCode.ToString(), "ExecuteCommand");
        }
        

        private void btn_start_compress_Click(object sender, RoutedEventArgs e)
        {
            if (tb_source.Text.Length == 0)
            {
                MessageBox.Show("Soure Directory is not selected.");
                return;
            }



            if (tb_target.Text.Length == 0)
            {
                MessageBox.Show("Target Directory is not selected.");
                return;
            }

            if (!Directory.Exists(tb_source.Text))
            {
                MessageBox.Show("Soure Directory not exist.");
                return;
            }
            if (!Directory.Exists(tb_target.Text))
            {
                Directory.CreateDirectory(tb_target.Text);
            }

            Boolean enableYUICompress = false;
            Boolean enableHTMLCompress = false;

            if (tb_htmlc.Text.Length == 0)
            {
                MessageBox.Show("html compressor & yui compressor are not selected.");
                return;
            }
            if (tb_htmlc.Text.Length == 0)
            {
                MessageBox.Show("htmlcompressor is not selected, HTML compression will be ignored.");
            }
            else { enableHTMLCompress = true; }
            if (tb_yui.Text.Length == 0)
            {
                MessageBox.Show("Yui Compressor is not selected, JS/CSS compression will be ignored.");
            }
            else
            {
                enableYUICompress = true;
            }


            String[] subdirs = Directory.GetDirectories(tb_source.Text, "*", SearchOption.AllDirectories);

            //Create sub directory to target if not exist
            foreach (var dir in subdirs)
            {
                String new_dir = dir.Replace(tb_source.Text, tb_target.Text);
                if (!Directory.Exists(new_dir)) Directory.CreateDirectory(new_dir);
            }
            //Compress files
            String[] files = Directory.GetFileSystemEntries(tb_source.Text, "*", SearchOption.AllDirectories);
            String temp_bat_file = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "temp.bat");
            using (StreamWriter w = new System.IO.StreamWriter(temp_bat_file))
            {
                foreach (String src_file in files)
                {
                    FileAttributes attr = File.GetAttributes(src_file);
                    if (!attr.HasFlag(FileAttributes.Directory)) //check if it is a file
                    {
                        String target_file = src_file.Replace(tb_source.Text, tb_target.Text);
                        String ext = System.IO.Path.GetExtension(src_file);
                        if ((ext == ".htm" || ext == ".html") && enableHTMLCompress)
                        {
                            //execute_jar(tb_htmlc.Text + " --compress-js " + src_file + " > " + target_file);
                            w.WriteLine("java -jar " + tb_htmlc.Text + " --compress-js " + src_file + " > " + target_file);
                        }
                        else if ((ext == ".js" || ext == ".css") && enableYUICompress)
                        {
                            //execute_jar(tb_yui.Text + " " + src_file + " -o " + target_file);
                            w.WriteLine("java -jar " + tb_yui.Text + " " + src_file + " -o " + target_file);
                        }
                        //else if (ext == ".css" && enableYUICompress)
                        //{

                        //}
                        else
                        {
                            //copy file to target directory

                            File.Copy(src_file, target_file, true);
                        }

                    }
                }
            }

            ExecuteCommand(temp_bat_file);

            File.Delete(temp_bat_file);

            MessageBox.Show("Completed");
        }

        private void btn_yui_download_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/yui/yuicompressor/releases");
        }

        private void btn_htmc_download_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://code.google.com/p/htmlcompressor/downloads");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.HTTPCOMPRESSOR_PATH = tb_htmlc.Text;
            Properties.Settings.Default.YUICOMPRESSOR_PATH = tb_yui.Text;
            Properties.Settings.Default.SOURCE_PATH = tb_source.Text;
            Properties.Settings.Default.TARGET_PATH = tb_target.Text;
            Properties.Settings.Default.Save();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tb_htmlc.Text = Properties.Settings.Default.HTTPCOMPRESSOR_PATH;
            tb_yui.Text = Properties.Settings.Default.YUICOMPRESSOR_PATH;
            tb_source.Text = Properties.Settings.Default.SOURCE_PATH;
            tb_target.Text = Properties.Settings.Default.TARGET_PATH;
        }




    }
}
