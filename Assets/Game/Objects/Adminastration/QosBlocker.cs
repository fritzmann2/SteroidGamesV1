using UnityEngine;


public class QosBlocker : MonoBehaviour
{
    private ILogHandler defaultLogHandler;

    private void Awake()
    {
        // Wir merken uns den originalen Unity-Logger
        defaultLogHandler = Debug.unityLogger.logHandler;
        
        // Wir tauschen ihn gegen unseren eigenen Filter aus
        Debug.unityLogger.logHandler = new QosLogSilencer(defaultLogHandler);
        
        // Optional: Damit der Filter auch beim Szenenwechsel aktiv bleibt
        DontDestroyOnLoad(gameObject);
    }

    // Wenn das Objekt zerstört wird, geben wir Unity seinen normalen Logger zurück
    private void OnDestroy()
    {
        if (Debug.unityLogger.logHandler is QosLogSilencer)
        {
            Debug.unityLogger.logHandler = defaultLogHandler;
        }
    }

    // ==========================================
    // UNSER EIGENER LOGGER
    // ==========================================
    private class QosLogSilencer : ILogHandler
    {
        private ILogHandler originalHandler;

        public QosLogSilencer(ILogHandler original)
        {
            originalHandler = original;
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            // Wir bauen die Nachricht zusammen, um sie zu prüfen
            string message = (args != null && args.Length > 0) ? string.Format(format, args) : format;

            // HIER IST DER FILTER: Wenn das Wort drin vorkommt -> unsichtbar machen!
            if (message.Contains("QosJob") || message.Contains("QoS"))
            {
                return; // Wir brechen ab, die Nachricht wird verschluckt.
            }

            // Wenn es eine normale Nachricht ist, geben wir sie ganz normal an Unity weiter
            originalHandler.LogFormat(logType, context, format, args);
        }

        public void LogException(System.Exception exception, Object context)
        {
            // Echte Fehler lassen wir natürlich immer durch
            originalHandler.LogException(exception, context);
        }
    }
}