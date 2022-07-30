namespace Trust.Logging;

/// <summary>
/// Logging severity levels.  Used to filter logging.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Logs with extreme detail and frequency. Enable only as a last resort when incredible spam may help investigations.
    /// </summary>
    Verbose,

    /// <summary>
    /// Logs with extra detail to aid debugging and development.
    /// </summary>
    Debug,

    /// <summary>
    /// Logs that communicate the general application flow to both end-users and developers.
    /// </summary>
    Information,

    /// <summary>
    /// Logs that cover non-blocking issues with application flow.
    /// </summary>
    Warning,

    /// <summary>
    /// Logs that cover blocking issues that halt application flow.
    /// </summary>
    Error,

    /// <summary>
    /// Logs absolutely nothing.
    /// </summary>
    None,
}
