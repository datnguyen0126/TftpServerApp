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
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;
using TftpServerApp.Code;

namespace TftpServerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker worker;
        private bool keepGoing = true;
        private TftpClient tftpClient;
        private TftpServer tftpServer;
        private Thread transferThread;

        public MainWindow()
        {
            InitializeComponent();
            ToggleState();
        }
        private void ServerBtn_Click(object sender, RoutedEventArgs e)
        {
            ToggleState();

            if (keepGoing)
            {

                worker = new BackgroundWorker();
                worker.WorkerSupportsCancellation = true;
                worker.WorkerReportsProgress = true;

                tftpServer = new TftpServer();
                tftpServer.Path = folderServer.Text;
                worker.DoWork += tftpServer.StartTftpServices;

                worker.RunWorkerAsync();
            }
            else
            {
                if (worker != null)
                {
                    tftpServer.StopTftopService();
                    worker.CancelAsync();
                }
            }     
        }
        private void ToggleState()
        {
            keepGoing = !keepGoing;

            // Enable/disable buttons  
            if (!keepGoing) serverBtn.Content = "Start server";
            else serverBtn.Content = "Stop";
        }

        private void getBtn_Click(object sender, RoutedEventArgs e)
        {
            string fileName = filenameBox.Text;
            string path = pathBox.Text;

            TftpClient tftpClient = new TftpClient(addressBox.Text, Int32.Parse(portBox.Text));
            tftpClient.Get(fileName, path + fileName);
        }

        private void putBtn_Click(object sender, RoutedEventArgs e)
        {
            string fileName = filenameBox.Text;
            string path = pathBox.Text;

            TftpClient tftpClient = new TftpClient(addressBox.Text, Int32.Parse(portBox.Text));
            tftpClient.Put(fileName, path + fileName);

        }
    }
}
