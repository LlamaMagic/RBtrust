using System;
using System.Windows.Media;
using Trust.Localization;

namespace Trust.Logging;

/// <summary>
/// Custom logger that writes to bot logs + console and general terminal.
/// </summary>
public static class Logger
{
    /// <summary>
    /// Display name for this log category.  Appears at the start of each log line.
    /// </summary>
    private static readonly string Name = Translations.PROJECT_NAME;

    /// <summary>
    /// Gets or sets <see cref="System.Windows.Media.Color"/> of log lines displayed in bot console.
    /// </summary>
    public static Color Color { get; set; } = Colors.Aqua;

    /// <summary>
    /// Gets or sets the current <see cref="Logging.LogLevel"/> for log filtering. Logs will include
    /// current level and above (e.g. Information -> Information through Critical).
    /// </summary>
    public static LogLevel LogLevel { get; set; }

    /// <summary>
    /// Checks if <see cref="LogLevel"/> will print from this <see cref="Logger"/>.
    /// </summary>
    /// <param name="logLevel"><see cref="LogLevel"/> to evaluate.</param>
    /// <returns><see langword="true"/> if enabled.</returns>
    public static bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel;
    }

    /// <summary>
    /// Writes a message to log with the indicated color, regardless of <see cref="LogLevel"/>.
    /// </summary>
    /// <param name="color">Log line <see cref="System.Windows.Media.Color"/>.</param>
    /// <param name="message">Text to write to log.</param>
    /// <param name="args">Array of zero or more <see cref="object"/>s to format the message with.</param>
    public static void WriteLog(Color color, string message, params object[] args)
    {
        string logLine = $"[{Name}] {string.Format(message, args)}";

        ff14bot.Helpers.Logging.Write(color, logLine);
        Console.WriteLine(logLine);  // Needed to appear in debugger, tests, etc
    }

    /// <summary>
    /// Writes a message to log, filtered by <see cref="LogLevel"/>.
    /// </summary>
    /// <param name="logLevel">Severity of this message.</param>
    /// <param name="color">Log line <see cref="System.Windows.Media.Color"/>.</param>
    /// <param name="message">Text to write to log.</param>
    /// <param name="args">Array of zero or more <see cref="object"/>s to format the message with.</param>
    public static void WriteFilteredLog(LogLevel logLevel, Color color, string message, params object[] args)
    {
        if (IsEnabled(logLevel))
        {
            WriteLog(color, message, args);
        }
    }

    /// <summary>
    /// Writes a <see cref="LogLevel.Verbose"/> message to log.
    /// </summary>
    /// <param name="message">Text to write to log.</param>
    /// <param name="args">Array of zero or more <see cref="object"/>s to format the message with.</param>
    public static void Verbose(string message, params object[] args)
    {
        WriteFilteredLog(LogLevel.Verbose, Color, message, args);
    }

    /// <summary>
    /// Writes a <see cref="LogLevel.Debug"/> message to log.
    /// </summary>
    /// <param name="message">Text to write to log.</param>
    /// <param name="args">Array of zero or more <see cref="object"/>s to format the message with.</param>
    public static void Debug(string message, params object[] args)
    {
        WriteFilteredLog(LogLevel.Debug, Color, message, args);
    }

    /// <summary>
    /// Writes an <see cref="LogLevel.Information"/> message to log.
    /// </summary>
    /// <param name="message">Text to write to log.</param>
    /// <param name="args">Array of zero or more <see cref="object"/>s to format the message with.</param>
    public static void Information(string message, params object[] args)
    {
        WriteFilteredLog(LogLevel.Information, Color, message, args);
    }

    /// <summary>
    /// Writes a <see cref="LogLevel.Warning"/> message to log.
    /// </summary>
    /// <param name="message">Text to write to log.</param>
    /// <param name="args">Array of zero or more <see cref="object"/>s to format the message with.</param>
    public static void Warning(string message, params object[] args)
    {
        WriteFilteredLog(LogLevel.Warning, Colors.Goldenrod, message, args);
    }

    /// <summary>
    /// Writes an <see cref="LogLevel.Error"/> message to log.
    /// </summary>
    /// <param name="message">Text to write to log.</param>
    /// <param name="args">Array of zero or more <see cref="object"/>s to format the message with.</param>
    public static void Error(string message, params object[] args)
    {
        WriteFilteredLog(LogLevel.Error, Colors.OrangeRed, message, args);
    }
}
