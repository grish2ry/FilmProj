using Presentation;
namespace Infrostruture.Loggers;
public class AppLogger : IAppLogger
{
    public IUi ui {get; private set;}

    public AppLogger(IUi ui)
    {
        this.ui = ui;
    }
    public void LogInfoMessage(string msg)
    {
        ui.PrintSingle($"[INFO] : {msg}");
    }

    public void LogErrorMessage(string msg)
    {
        ui.PrintSingle($"[ERROR] : {msg}");
    }

    public void LogWarningMessage(string msg)
    {
        ui.PrintSingle($"[WARNING] : {msg}");
    }
}