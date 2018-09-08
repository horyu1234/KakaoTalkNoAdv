using System;
using Topshelf;

namespace KakaoTalkNoAdv
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var host = HostFactory.Run(config =>
            {
                config.Service<AdRemover>(service =>
                {
                    service.ConstructUsing(name => new AdRemover());
                    service.WhenStarted(remover => remover.Start());
                    service.WhenStopped(remover => remover.Stop());
                    service.WhenContinued(remover => remover.Start());
                    service.WhenPaused(remover => remover.Stop());
                });
                config.RunAsLocalSystem();

                config.SetDisplayName("카카오톡 PC 버전 광고 제거");
                config.SetServiceName("KakaoTalkNoAdv");
                config.SetDescription("카카오톡 PC 버전 광고 제거 by horyu1234, kiwiyou" +
                                      Environment.NewLine + "Special Thanks to real21c");
            });
            var exitCode = (int)Convert.ChangeType(host, host.GetTypeCode());
            Environment.ExitCode = exitCode;
        }
    }
}