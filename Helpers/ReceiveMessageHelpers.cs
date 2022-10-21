using ff14bot.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Trust.Logging;

namespace Trust.Helpers;

/// <summary>
/// ReceiveMessageHelpers class.
/// </summary>
internal static class ReceiveMessageHelpers
{
    /// <summary>
    /// SkillsdeterminationOverStatus.
    /// </summary>
    public static bool SkillsdeterminationOverStatus;

    /// <summary>
    /// SkillsdeterminationOverStr.
    /// </summary>
    public static HashSet<string> SkillsdeterminationOverStr = new();

    private static readonly Regex LosesEffectRegex = new("loses the effect of (.*?)", RegexOptions.None);

    private static string wfnpcAcmtsstr;
    private static string vcNpcAcmtsstr;
    private static bool magnetOverStatus;
    private static bool vcNpcAcmtsstrStatus;
    private static bool wfnpcAcmtsstrStatus;
    private static bool skillsdetStatus;

    private static HashSet<string> magnetOverStr = new();

    private static HashSet<string> Skillsdetstr { get; set; } = new();

    /// <summary>
    /// ReceiveMessage.
    /// </summary>
    /// <param name="sender">?.</param>
    /// <param name="args">args.</param>
    public static void ReceiveMessage(object sender, ChatEventArgs args)
    {
        NPCAcmts(args);
        WFNPCAcmts(args);

        if ((int)args.ChatLogEntry.MessageType <= 8774
            || LosesEffectRegex.IsMatch(args.ChatLogEntry.FullLine)
            || args.ChatLogEntry.FullLine.Contains("⇒"))
        {
            return;
        }

        SkillsdeterminationStart(args.ChatLogEntry.FullLine);
        SkillsdeterminationOver(args.ChatLogEntry.FullLine);
        MagnetOver(args.ChatLogEntry.FullLine);
    }

    /// <summary>
    /// NPCAcmts.
    /// </summary>
    /// <param name="npcatstr">?.</param>
    public static void NPCAcmts(ChatEventArgs npcatstr)
    {
        if (npcatstr.ChatLogEntry.MessageType == ff14bot.Enums.MessageType.NPCAnnouncements)
        {
            if (vcNpcAcmtsstr != null)
            {
                vcNpcAcmtsstrStatus = npcatstr.ChatLogEntry.FullLine.Contains(vcNpcAcmtsstr);
            }
        }
    }

    /// <summary>
    /// WFNPCAcmts.
    /// </summary>
    /// <param name="npcatstr">?.</param>
    public static void WFNPCAcmts(ChatEventArgs npcatstr)
    {
        if (npcatstr.ChatLogEntry.MessageType == ff14bot.Enums.MessageType.NPCAnnouncements)
        {
            if (!string.IsNullOrEmpty(wfnpcAcmtsstr))
            {
                wfnpcAcmtsstrStatus = npcatstr.ChatLogEntry.FullLine.Contains(wfnpcAcmtsstr);
            }
        }
    }

    /// <summary>
    /// SkillsdetstrGet.
    /// </summary>
    /// <param name="goldChaser">?.</param>
    public static void SkillsdetstrGet(HashSet<uint> goldChaser)
    {
        IEnumerable<string> skstr = goldChaser?.Select(r => DataManager.GetSpellData(r).LocalizedName);

        Skillsdetstr = new HashSet<string>(skstr);
    }

    /// <summary>
    /// SkillsdeterminationStart.
    /// </summary>
    /// <param name="sderstr">?.</param>
    public static void SkillsdeterminationStart(string sderstr)
    {
        if (sderstr.Contains("readies") ||
                sderstr.Contains("begins casting"))
        {
            try
            {
                if (Skillsdetstr != null)
                {
                    skillsdetStatus = (bool)Skillsdetstr?.Any(r => sderstr.Contains(r));
                }
            }
            catch
            {
                Logger.Information($"亲信没有添加 判断技能");
            }
        }

        if (sderstr.Contains("casts") ||
                sderstr.Contains("uses"))
        {
            if (Skillsdetstr != null)
            {
                if ((bool)Skillsdetstr?.Any(r => sderstr.Contains(r)))
                {
                    skillsdetStatus = false;
                }
            }
        }
    }

    /// <summary>
    /// SkillsdeterminationOver.
    /// </summary>
    /// <param name="sderstr">?.</param>
    public static void SkillsdeterminationOver(string sderstr)
    {
        if (sderstr.Contains("uses") ||
                sderstr.Contains("casts"))
        {
            try
            {
                if ((bool)SkillsdeterminationOverStr?.Any())
                {
                    if (SkillsdeterminationOverStr.Any(str => sderstr.Contains(str)))
                    {
                        SkillsdeterminationOverStatus = true;
                    }
                }
            }
            catch
            {
                Logger.Information($"亲信没有添加 判断技能");
            }
        }
    }

    /// <summary>
    /// MagnetOverStrGet.
    /// </summary>
    /// <param name="magnet">?.</param>
    public static void MagnetOverStrGet(HashSet<uint> magnet)
    {
        IEnumerable<string> str = magnet?.Select(r => DataManager.GetSpellData(r).LocalizedName);

        magnetOverStr = new HashSet<string>(str);
    }

    /// <summary>
    /// MagnetOver.
    /// </summary>
    /// <param name="sderstr">?.</param>
    public static void MagnetOver(string sderstr)
    {
        if (sderstr.Contains("uses") ||
                sderstr.Contains("casts"))
        {
            try
            {
                if ((bool)magnetOverStr?.Any())
                {
                    magnetOverStatus = (bool)magnetOverStr?.Any(r => sderstr.Contains(r));
                }
            }
            catch
            {
                Logger.Information($"亲信没有添加 判断技能");
            }
        }
    }
}
