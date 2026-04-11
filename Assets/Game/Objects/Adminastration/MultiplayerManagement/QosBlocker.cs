using UnityEngine;


public class QosBlocker : MonoBehaviour
{
    private ILogHandler defaultLogHandler;

    private void Awake()
    {
        defaultLogHandler = Debug.unityLogger.logHandler;
        Debug.unityLogger.logHandler = new QosLogSilencer(defaultLogHandler);
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Debug.unityLogger.logHandler is QosLogSilencer)
        {
            Debug.unityLogger.logHandler = defaultLogHandler;
        }
    }

    private class QosLogSilencer : ILogHandler
    {
        private ILogHandler originalHandler;

        public QosLogSilencer(ILogHandler original)
        {
            originalHandler = original;
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            string message = (args != null && args.Length > 0) ? string.Format(format, args) : format;
            if (message.Contains("QosJob") || message.Contains("QoS") || message.Contains("GameView reduced"))
            {
                return; 
            }
            originalHandler.LogFormat(logType, context, format, args);
        }

        public void LogException(System.Exception exception, Object context)
        {
            originalHandler.LogException(exception, context);
        }
    }
}