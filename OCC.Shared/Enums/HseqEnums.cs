namespace OCC.Shared.Enums
{
    public enum IncidentType
    {
        Injury,
        NearMiss,
        PropertyDamage,
        Environmental,
        Fire,
        Theft,
        Other
    }

    public enum IncidentSeverity
    {
        Low,
        Medium,
        High,
        Fatality,
        Critical
    }

    public enum IncidentStatus
    {
        Open,
        Investigating,
        Closed,
        Archived
    }

    public enum AuditStatus
    {
        Open,
        Scheduled,
        InProgress,
        Completed,
        Closed
    }

    public enum AuditItemStatus
    {
        Open,
        Rectified,
        Closed
    }

    public enum DocumentCategory
    {
        Policy,
        Template,
        Procedure,
        Form,
        Report,
        Other
    }
}
