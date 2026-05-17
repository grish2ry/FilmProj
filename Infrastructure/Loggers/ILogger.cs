using Presentation;

namespace Infrostruture.Loggers;
public interface IAppLogger
{
    public void LogInfoMessage(string msg);

    public void LogErrorMessage(string msg);

    public void LogWarningMessage(string msg);
}