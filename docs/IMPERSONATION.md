# User Impersonation Feature Documentation

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Implementation Details](#implementation-details)
4. [API Endpoints](#api-endpoints)
5. [Frontend Components](#frontend-components)
6. [Usage Guide](#usage-guide)
7. [Security Considerations](#security-considerations)
8. [Testing](#testing)
9. [Troubleshooting](#troubleshooting)

## Overview

The User Impersonation feature allows administrators to temporarily act as another user within the SearchBugs application. This is useful for debugging issues, providing user support, or testing user-specific functionality.

### Key Features

- **Admin-Only Access**: Only users with Admin role can impersonate other users
- **JWT-Based Implementation**: Uses custom JWT claims to maintain impersonation state
- **Visual Indicators**: Clear UI feedback when impersonation is active
- **Easy Toggle**: Simple start/stop impersonation functionality
- **Audit Trail**: All actions are performed under the impersonated user's context

## Architecture

### Backend Components

#### 1. JWT Provider (`JwtProvider.cs`)

Generates JWT tokens with impersonation claims:

- `impersonated_user_id`: ID of the user being impersonated
- `original_user_id`: ID of the actual logged-in user
- `impersonated_email`: Email of the impersonated user

#### 2. Current User Service (`CurrentUserService.cs`)

Handles user context with impersonation support:

- **UserId**: Returns impersonated user ID when active, otherwise returns actual user ID
- **Email**: Returns impersonated user email when active
- **IsImpersonating**: Boolean indicating if impersonation is active
- **OriginalUserId**: Returns the actual logged-in user ID
- **ActualUserId**: Returns original user ID if impersonating, otherwise current user ID

#### 3. Command Handlers

- **ImpersonateCommandHandler**: Handles starting impersonation
- **StopImpersonateCommandHandler**: Handles stopping impersonation

### Frontend Components

#### 1. Auth Context (`AuthContext.tsx`)

Extended to support impersonation:

```typescript
interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  isImpersonating?: boolean;
  originalUserId?: string;
  originalUserEmail?: string;
}
```

#### 2. UI Components

- **ImpersonationDialog**: Modal for selecting users to impersonate
- **ImpersonationBanner**: Visual indicator when impersonation is active

## Implementation Details

### JWT Token Structure

#### Regular Token

```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "exp": "expiration-time",
  "iss": "issuer",
  "aud": "audience"
}
```

#### Impersonation Token

```json
{
  "sub": "original-user-id",
  "email": "original-user@example.com",
  "original_user_id": "original-user-id",
  "impersonated_user_id": "target-user-id",
  "impersonated_email": "target-user@example.com",
  "exp": "expiration-time",
  "iss": "issuer",
  "aud": "audience"
}
```

### Current User Service Logic

```csharp
public UserId UserId
{
    get
    {
        // Check for impersonation first
        var impersonatedUserIdClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst(ImpersonatedUserIdClaimType);
        if (impersonatedUserIdClaim != null &&
            Guid.TryParse(impersonatedUserIdClaim.Value, out var impersonatedUserId))
        {
            return new UserId(impersonatedUserId);
        }

        // Fall back to regular user ID claims
        var userIdClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier) ??
            _httpContextAccessor.HttpContext?.User
            .FindFirst(JwtRegisteredClaimNames.Sub);

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        return new UserId(userId);
    }
}
```

## API Endpoints

### Start Impersonation

```http
POST /api/auth/impersonate
Authorization: Bearer {admin-jwt-token}
Content-Type: application/json

{
  "userIdToImpersonate": "target-user-guid"
}
```

**Response:**

```json
{
  "isSuccess": true,
  "value": {
    "token": "new-impersonation-jwt-token",
    "impersonatedUserEmail": "target-user@example.com"
  }
}
```

### Stop Impersonation

```http
POST /api/auth/stop-impersonate
Authorization: Bearer {impersonation-jwt-token}
```

**Response:**

```json
{
  "isSuccess": true,
  "value": {
    "token": "original-user-jwt-token"
  }
}
```

## Frontend Components

### ImpersonationDialog Component

```tsx
// Located at: src/components/ImpersonationDialog.tsx
// Features:
// - User search functionality
// - List of available users
// - Click to impersonate
// - Only visible to admin users
// - Hidden when already impersonating
```

### ImpersonationBanner Component

```tsx
// Located at: src/components/ImpersonationBanner.tsx
// Features:
// - Orange warning banner
// - Shows impersonated user email
// - Shows original user email
// - Stop impersonation button
// - Only visible when impersonating
```

### Auth Context Integration

```tsx
// Extended AuthContextType interface:
interface AuthContextType {
  user: User | null;
  login: (email: string, password: string) => Promise<void>;
  register: (...) => Promise<void>;
  logout: () => void;
  impersonate: (userId: string) => Promise<void>;      // New
  stopImpersonate: () => Promise<void>;                // New
  isAuthenticated: boolean;
  isLoading: boolean;
}
```

## Usage Guide

### For Administrators

#### 1. Prerequisites

- Login with an admin account
- Ensure you have the "Admin" role assigned
- Navigate to any page in the application

#### 2. Starting Impersonation

1. Look for the "Impersonate User" button in the header
2. Click the button to open the impersonation dialog
3. Use the search box to find a user by name or email
4. Click "Impersonate" next to the desired user
5. The dialog closes and an orange banner appears

#### 3. While Impersonating

- Orange banner shows at the top of all pages
- All actions are performed as the impersonated user
- API calls use the impersonated user's context
- The "Impersonate User" button is hidden

#### 4. Stopping Impersonation

1. Click "Stop Impersonating" in the orange banner
2. The banner disappears
3. You return to your original admin session
4. The "Impersonate User" button reappears

### For Developers

#### Backend Development

```csharp
// Access current user (impersonated if active)
var currentUserId = _currentUserService.UserId;

// Access actual logged-in user (for audit trails)
var actualUserId = _currentUserService.ActualUserId;

// Check if impersonating
if (_currentUserService.IsImpersonating)
{
    var originalUserId = _currentUserService.OriginalUserId;
    // Handle impersonation-specific logic
}
```

#### Frontend Development

```tsx
// Access impersonation state
const { user, impersonate, stopImpersonate } = useAuth();

// Check if impersonating
if (user?.isImpersonating) {
  // Show impersonation-specific UI
  console.log(`Impersonating: ${user.email}`);
  console.log(`Original user: ${user.originalUserEmail}`);
}

// Start impersonation
await impersonate(targetUserId);

// Stop impersonation
await stopImpersonate();
```

## Security Considerations

### 1. Authorization

- Only users with "Admin" role can start impersonation
- JWT tokens contain original user information for audit purposes
- Impersonation tokens have the same expiration as regular tokens

### 2. Audit Trail

- All actions performed during impersonation are logged under the impersonated user
- Original user information is preserved in JWT claims
- Backend services can access both current and original user IDs

### 3. Token Security

- Impersonation tokens are signed with the same secret as regular tokens
- Tokens include both original and impersonated user information
- Standard JWT security practices apply

### 4. Permission Inheritance

- Impersonated sessions inherit the target user's permissions
- Admin users temporarily lose their admin privileges while impersonating
- Role-based access control continues to work normally

## Testing

### Backend Tests

```csharp
// Test impersonation command handler
[Fact]
public async Task Handle_ValidImpersonation_ReturnsToken()
{
    // Arrange
    var originalUser = CreateTestUser("admin@test.com", "Admin");
    var targetUser = CreateTestUser("user@test.com", "User");
    var command = new ImpersonateCommand(targetUser.Id.Value);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Token.Should().NotBeNullOrEmpty();
    result.Value.ImpersonatedUserEmail.Should().Be("user@test.com");
}

// Test current user service
[Fact]
public void UserId_WhenImpersonating_ReturnsImpersonatedUserId()
{
    // Setup claims with impersonation
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, "original-user-id"),
        new Claim("impersonated_user_id", "target-user-id")
    };

    // Test that UserId returns impersonated user ID
    var userId = currentUserService.UserId;
    userId.Value.Should().Be(Guid.Parse("target-user-id"));
}
```

### Frontend Tests

```tsx
// Test impersonation dialog
describe("ImpersonationDialog", () => {
  it("shows user list when opened", async () => {
    render(<ImpersonationDialog />);

    fireEvent.click(screen.getByText("Impersonate User"));

    await waitFor(() => {
      expect(screen.getByText("Search Users")).toBeInTheDocument();
    });
  });

  it("calls impersonate function when user is selected", async () => {
    const mockImpersonate = jest.fn();

    // Mock useAuth hook
    jest.mocked(useAuth).mockReturnValue({
      impersonate: mockImpersonate,
      // ... other auth properties
    });

    render(<ImpersonationDialog />);

    fireEvent.click(screen.getByText("Impersonate"));

    expect(mockImpersonate).toHaveBeenCalledWith("target-user-id");
  });
});

// Test impersonation banner
describe("ImpersonationBanner", () => {
  it("shows when user is impersonating", () => {
    jest.mocked(useAuth).mockReturnValue({
      user: {
        id: "target-user-id",
        email: "target@test.com",
        isImpersonating: true,
        originalUserEmail: "admin@test.com",
        // ... other properties
      },
      // ... other auth properties
    });

    render(<ImpersonationBanner />);

    expect(screen.getByText(/You are impersonating/)).toBeInTheDocument();
    expect(screen.getByText("target@test.com")).toBeInTheDocument();
    expect(screen.getByText(/admin@test.com/)).toBeInTheDocument();
  });
});
```

## Troubleshooting

### Common Issues

#### 1. "Impersonate User" button not visible

- **Cause**: User doesn't have Admin role
- **Solution**: Verify user has "Admin" role assigned
- **Check**: User roles in database or user management interface

#### 2. Impersonation fails with 403 Forbidden

- **Cause**: Missing authorization or insufficient permissions
- **Solution**: Ensure JWT token is valid and user has Admin role
- **Debug**: Check JWT claims and user permissions

#### 3. Impersonation banner not appearing

- **Cause**: JWT token doesn't contain impersonation claims
- **Solution**: Verify impersonation token generation and storage
- **Debug**: Decode JWT token and check for impersonation claims

#### 4. API calls still using original user

- **Cause**: CurrentUserService not properly reading impersonation claims
- **Solution**: Verify claim types match between JWT generation and reading
- **Debug**: Check JWT claims and CurrentUserService implementation

### Debug Steps

#### 1. JWT Token Inspection

```javascript
// In browser console
const token = localStorage.getItem("access");
const payload = JSON.parse(atob(token.split(".")[1]));
console.log("JWT Payload:", payload);
console.log("Is Impersonating:", !!payload.impersonated_user_id);
```

#### 2. Backend Debugging

```csharp
// Add logging to CurrentUserService
private readonly ILogger<CurrentUserService> _logger;

public UserId UserId
{
    get
    {
        var claims = _httpContextAccessor.HttpContext?.User.Claims;
        _logger.LogInformation("Available claims: {Claims}",
            string.Join(", ", claims.Select(c => $"{c.Type}: {c.Value}")));

        // ... rest of implementation
    }
}
```

#### 3. Network Request Inspection

- Open browser DevTools → Network tab
- Make an API request while impersonating
- Check request headers for correct Authorization bearer token
- Verify token contains impersonation claims

### Error Messages and Solutions

| Error                       | Cause                                             | Solution                                 |
| --------------------------- | ------------------------------------------------- | ---------------------------------------- |
| "User is not authenticated" | Invalid or missing JWT token                      | Re-login or refresh token                |
| "Impersonation failed"      | Target user not found or insufficient permissions | Check user exists and has admin role     |
| "Stop impersonation failed" | Not currently impersonating                       | Verify impersonation state               |
| UI components not rendering | Missing dependencies or incorrect props           | Check component imports and auth context |

## Changelog

### Version 1.0.0 (Current)

- Initial implementation of user impersonation feature
- JWT-based impersonation with custom claims
- Frontend components for impersonation dialog and banner
- Admin-only access control
- Full integration with existing authentication system

### Future Enhancements

- Permission-based impersonation (specific permissions instead of just Admin role)
- Time-limited impersonation sessions
- Impersonation audit logs
- Bulk impersonation capabilities for testing
- Integration with external identity providers

---

_Last Updated: August 30, 2025_
_Documentation Version: 1.0.0_
