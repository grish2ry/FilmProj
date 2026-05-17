using Infrostruture.Loggers;
using Presentation;
using System.Diagnostics;

namespace AppFilm.Helpers;
public class AppHelper
{
    public AppLogger Logger {get; private set;}
    public AppHelper(IUi ui)
    {
        Logger = new AppLogger(ui);
    }
    public void ReaderDiagnostics(string name, Action action)
    {
        var process = Process.GetCurrentProcess();
        var cpuBef = process.TotalProcessorTime;
        var sw = Stopwatch.StartNew();
        action();
        sw.Stop();
        var cpuAfter = process.TotalProcessorTime;
        Logger.LogInfoMessage($"[{name}] : time = {sw.ElapsedMilliseconds}");
        Logger.LogInfoMessage($"[{name}] : cpu = {(cpuAfter-cpuBef).TotalMilliseconds}");
    }   
}