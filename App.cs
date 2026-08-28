using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace ProcessorEmulator
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Win7VisualStyle.EnableHost();
            base.OnStartup(e);

            if (e.Args != null && e.Args.Length > 0 && e.Args[0] == "--test-uverse")
            {
                UverseEmulatorTest.RunTest().GetAwaiter().GetResult();
                Shutdown();
                return;
            }

            string feed = "";
            if (e.Args != null)
            {
                for (int i = 0; i < e.Args.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(e.Args[i]) && e.Args[i][0] != '-')
                    {
                        feed = e.Args[i];
                        break;
                    }
                }
            }

            var host = new MediaroomHostForm();
            if (!string.IsNullOrEmpty(feed))
                host.DiskFolder = feed;
            System.Windows.Forms.Application.Run(host);
            Shutdown();
        }
    }
}
