using ProcessRunner;
using ProcessRunner.Diagnostics;
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

namespace ProcessRunnerUI
{
    public partial class MainWindow : Window
    {
        private RunnerBase m_runner;

        public MainWindow()
        {
            m_runner = new RunnerBase("cmd.exe", "cmd");
            m_runner.StopProcessWhenDispose = true;
            m_runner.RestartAfterUnexpectedShutdown = true;

            var logger = new Logger();
            logger.Info("Some text");

            InitializeComponent();
        }

        private async void BStart_OnClick(object sender, RoutedEventArgs e)
        {
            await m_runner.StartAsync();
        }

        private async void BRestart_OnClick(object sender, RoutedEventArgs e)
        {
            await m_runner.RestartAsync();
        }

        private async void BStop_OnClick(object sender, RoutedEventArgs e)
        {
            await m_runner.StopAsync();
        }
    }
}
