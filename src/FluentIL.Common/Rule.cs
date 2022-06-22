namespace FluentIL.Common
{
    public enum RuleSeverity
    {
        Hidden,
        Info,
        Warning,
        Error
    }

    public class Rule
    {

        public Rule(string id, string title, string message, RuleSeverity severity, string description, string helpLinkUri)
        {
            Id = id;
            Title = title;
            Message = message;
            Severity = severity;
            Description = description;
            HelpLinkUri = helpLinkUri;
        }

        public string Id { get; }
        public string Title { get; }
        public string Message { get; }
        public RuleSeverity Severity { get; }
        public string Description { get; }
        public string HelpLinkUri { get; }
    }
}
