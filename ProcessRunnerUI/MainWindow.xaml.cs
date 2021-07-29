using ProcessRunner;
using ProcessRunner.Diagnostics;
using ProcessRunnerCore.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private readonly SynchronizationContext m_syncContext;
        private RunnerBase m_runner;
        private GlobalConfig m_appConfig = new GlobalConfig();

        public MainWindow()
        {
            m_syncContext = SynchronizationContext.Current;

            m_runner = new RunnerBase("cmd.exe", "cmd");
            m_runner.OnMessageReceived += (msg) =>
            {
                m_syncContext.Post(obj =>
                {
                    ConsoleOutput.Items.Add(msg);
                }, null);
            };
            m_runner.RunnerInfo.RestartAfterUnexpectedShutdown = true;

            InitializeComponent();
        }

        private async void BStart_OnClick(object sender, RoutedEventArgs e)
        {
            m_runner.Start();
            m_runner.Start();
        }

        private async void BRestart_OnClick(object sender, RoutedEventArgs e)
        {
            await m_runner.Restart();
        }

        private async void BStop_OnClick(object sender, RoutedEventArgs e)
        {
            await m_runner.Stop();
        }

        private void BCreateRunner_OnClick(object sender, RoutedEventArgs e)
        {
            //RunnerConfig config = new RunnerConfig(tbRunnerName.Text);

            //if (m_appConfig.AddConfig(config))
            //{
            //    config.Save();
            //    m_appConfig.Save();
            //}

            var msg = tbRunnerName.Text;
            m_runner.SendMessage(msg);
        }
    }
}
