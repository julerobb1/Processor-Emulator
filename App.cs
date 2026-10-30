using ProcessorEmulator.Core;
using ProcessorEmulator.Core;
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

            if (e.Args != null && e.Args.Length >= 2 && e.Args[0] == "--pace-hog")
            {
                // PACE_HOG: --pace-hog <PX013 image> [capSeconds=600] [log]
                int hcap = 600;
                if (e.Args.Length >= 3) int.TryParse(e.Args[2], out hcap);
                string hlog = e.Args.Length >= 4 ? e.Args[3]
                    : System.IO.Path.Combine(System.IO.Path.GetDirectoryName(e.Args[1]) ?? ".", "hog-run.log");
                PaceHogSession.Run(e.Args[1], hcap, hlog);
                Shutdown();
                return;
            }
            if (e.Args != null && e.Args.Length >= 2 && e.Args[0] == "--headless-boot")
            {
                // Headless Boot: --headless-boot <folder> [capSeconds]
                string hf = e.Args[1];
                int cap = 0; // 0 or omitted = no time limit
                if (e.Args.Length >= 3) int.TryParse(e.Args[2], out cap);
                BootLog.Open(hf);
                BootLog.Write("headless start folder=" + hf + " cap=" + cap + "s log=" + BootLog.FilePath);
                var hs = new MediaroomSession(BootLog.Write);
                var ht = new System.Threading.Thread(() =>
                {
                    try { hs.Run(hf); }
                    catch (Exception ex) { BootLog.Write("headless: " + ex.GetType().Name + ": " + ex.Message); }
                }) { IsBackground = true };
                ht.Start();
                if (!(cap > 0 ? ht.Join(TimeSpan.FromSeconds(cap)) : ht.Join(System.Threading.Timeout.Infinite)))
                {
                    BootLog.Write("headless cap reached; RequestStop");
                    hs.RequestStop();
                    ht.Join(TimeSpan.FromSeconds(10));
                }
                BootLog.Write("headless end");
                BootLog.UartFlush();
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
