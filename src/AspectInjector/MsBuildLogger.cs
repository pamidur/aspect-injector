using FluentIL.Common;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil.Cil;

namespace AspectInjector;

public class MsBuildLogger(TaskLoggingHelper log, bool verbose) : FluentIL.Logging.ILogger
{
    public virtual bool IsErrorThrown { get; private set; }

    public void Log(Rule rule, SequencePoint sp, params string[] parameters)
    {
        parameters ??= [];

        switch (rule.Severity)
        {
            case RuleSeverity.Error:
                IsErrorThrown = true;
                if (sp?.Document == null)
                {
                    log.LogError(rule.Message, parameters);
                }
                else
                {
                    log.LogError(
                        null, rule.Id, rule.Description, rule.HelpLinkUri,
                        sp.Document.Url, sp.StartLine, sp.StartColumn, sp.EndLine, sp.EndColumn,
                        rule.Message, parameters);
                }
                break;
            case RuleSeverity.Warning:
                if (sp?.Document == null)
                {
                    log.LogWarning(rule.Message, parameters);
                }
                else
                {
                    log.LogWarning(
                        null, rule.Id, rule.Description, rule.HelpLinkUri,
                        sp.Document.Url, sp.StartLine, sp.StartColumn, sp.EndLine, sp.EndColumn,
                        rule.Message, parameters);
                }
                break;
            case RuleSeverity.Info:
                if (sp?.Document == null)
                {
                    log.LogMessage(
                        verbose ? MessageImportance.High : MessageImportance.Normal,
                        rule.Message, parameters);
                }
                else
                {
                    log.LogMessage(
                        null, rule.Id, rule.Description,
                        sp.Document.Url, sp.StartLine, sp.StartColumn, sp.EndLine, sp.EndColumn,
                        verbose ? MessageImportance.High : MessageImportance.Normal,
                        rule.Message, parameters);
                }
                break;
        }
    }
}
