# Audit Logging Improvements

This document describes the improvements made to the audit logging system in SearchBugs.

## Features Implemented

### 1. Query Exclusion from Audit Logs

The audit logging system now only logs **commands (actions)** and **excludes queries** from being logged. This reduces noise in the audit logs and focuses on actions that modify system state.

**How it works:**

- The `AuditLoggingPipelineBehavior` checks if the request implements `IQuery<TResponse>` interface
- If it's a query, the request is processed without logging
- Only commands (implementing `ICommand` or `ICommand<TResponse>`) are logged

**Example:**

- `LoginCommand` ✅ **Logged** (it's an action)
- `GetBugsQuery` ❌ **Not Logged** (it's a query)
- `CreateBugCommand` ✅ **Logged** (it's an action)
- `GetUserDetailQuery` ❌ **Not Logged** (it's a query)

### 2. Sensitive Information Redaction with `[AuditIgnore]`

A new `AuditIgnoreAttribute` has been added that allows you to mark sensitive properties in Command/Query DTOs. Properties marked with this attribute will have their values replaced with `"****"` in the audit logs.

**Usage Example:**

```csharp
public sealed record LoginCommand(
    string Email,
    [property: AuditIgnore] string Password
) : ICommand<LoginResponse>;

public sealed record ChangePasswordCommand(
    Guid UserId,
    [property: AuditIgnore] string CurrentPassword,
    [property: AuditIgnore] string NewPassword
) : IRequest<Result>;
```

**Result in Audit Log:**

```json
{
  "email": "user@example.com",
  "password": "****"
}
```

### 3. Commands Already Updated

The following commands have been updated to use the `[AuditIgnore]` attribute:

- `LoginCommand` - Password field redacted
- `RegisterCommand` - Password field redacted
- `ChangePasswordCommand` - Both CurrentPassword and NewPassword fields redacted
- `CreateUserCommand` - Password field redacted

## Benefits

1. **Reduced Log Volume**: Queries are no longer logged, reducing audit log size significantly
2. **Enhanced Security**: Sensitive information like passwords are automatically redacted
3. **Better Compliance**: Helps meet security compliance requirements by not storing sensitive data in logs
4. **Improved Performance**: Less data written to audit logs means better performance
5. **Focused Auditing**: Audit logs now focus on actual actions/changes rather than data retrieval

## How to Use

### For Developers

When creating new commands that contain sensitive information:

1. Add the `using SearchBugs.Application.Common.Attributes;` directive
2. Mark sensitive properties with `[property: AuditIgnore]`

```csharp
using SearchBugs.Application.Common.Attributes;
using Shared.Messaging;

public sealed record MyCommand(
    string PublicData,
    [property: AuditIgnore] string SensitiveData,
    [property: AuditIgnore] string AnotherSecret
) : ICommand<MyResponse>;
```

### For Administrators

- Audit logs will now only show commands/actions
- Sensitive data will appear as `"****"` in the logs
- The audit log volume should be significantly reduced
- Query operations (like viewing bugs, users, etc.) are not logged

## Implementation Details

The feature is implemented through:

1. **AuditIgnoreAttribute** (`src/SearchBugs.Application/Common/Attributes/AuditIgnoreAttribute.cs`)
2. **Enhanced AuditLoggingPipelineBehavior** (`src/SearchBugs.Application/Common/Behaviors/AuditLoggingPipelineBehavior.cs`)
   - Added query detection logic
   - Added sensitive property redaction logic
   - Uses reflection to identify properties with `[AuditIgnore]` attribute

The system handles nested objects and collections properly, ensuring that sensitive data is redacted at any level of the object hierarchy.
