using System;

namespace SearchBugs.Application.Common.Attributes;

/// <summary>
/// Attribute to mark properties that should be excluded from audit logging.
/// Properties marked with this attribute will have their values replaced with "****" in audit logs.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class AuditIgnoreAttribute : Attribute
{
}
