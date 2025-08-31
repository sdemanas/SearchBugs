# SignalR Notification Testing Guide

## Overview

This guide explains how to test the real-time notification system implemented with SignalR in the SearchBugs application.

## Architecture Components

### Backend Components

- **NotificationHub.cs**: SignalR hub for real-time communication
- **NotificationService.cs**: Service for sending notifications
- **TestNotificationEndpoints.cs**: API endpoints for testing notifications
- **Domain Events**: Event-driven notification triggers

### Frontend Components

- **NotificationTester.tsx**: React component for testing notifications
- **NotificationTestPage.tsx**: Page wrapper for the tester component
- **useNotifications.ts**: React hook for notification management
- **notificationService.ts**: Service for SignalR connection management

## How to Use the Notification Tester

### 1. Start the Application

```bash
# Terminal 1: Start the backend API
cd /path/to/SearchBugs
dotnet run --project src/SearchBugs.Api

# Terminal 2: Start the React frontend
cd src/SearchBugs.Ui
npm run dev
```

### 2. Access the Test Interface

Navigate to: `http://localhost:5173/test-notifications`

### 3. Test Features Available

#### Connection Management

- **Connection Status**: Real-time display of SignalR connection state
- **Manual Connect/Disconnect**: Control SignalR connection manually
- **Auto-reconnection**: Automatic reconnection on connection loss

#### Test Notification Types

1. **Custom Test Notification**: Send a notification with custom message
2. **Bug Created Test**: Simulate a bug creation notification
3. **Bug Updated Test**: Simulate a bug status change notification

#### Monitoring Features

- **Real-time Results**: See immediate feedback from sent notifications
- **Notification Summary**: View all received notifications
- **Connection Health**: Monitor connection status and errors

### 4. Testing Workflow

1. **Check Connection**: Ensure SignalR is connected (green status)
2. **Set User ID**: Configure the target user ID (default: "test-user-123")
3. **Send Test Notification**:
   - Use custom message or select predefined types
   - Monitor the result display
   - Check the notification summary for received notifications

### 5. API Integration

The tester integrates with the backend API:

- **Endpoint**: `POST /api/test-notifications/send-test-notification`
- **Payload**: `{ userId: string, message?: string }`
- **Response**: Real-time notification via SignalR

## Troubleshooting

### Connection Issues

- Verify both backend (port 5026) and frontend (port 5173) are running
- Check browser console for SignalR connection errors
- Ensure CORS is properly configured

### Notification Not Received

- Verify the User ID matches between sender and receiver
- Check backend logs for notification service activity
- Confirm SignalR hub is properly registered

### API Errors

- Check backend API logs for endpoint errors
- Verify the test notification endpoint is accessible
- Ensure proper JSON payload format

## Technical Details

### SignalR Configuration

```csharp
// Backend hub registration
builder.Services.AddSignalR();
app.MapHub<NotificationHub>("/notificationHub");
```

### React Integration

```typescript
// Frontend SignalR connection
const connection = new HubConnectionBuilder()
  .withUrl("http://localhost:5026/notificationHub")
  .withAutomaticReconnect()
  .build();
```

### Real-time Flow

1. User clicks "Send Test Notification"
2. React app calls API endpoint
3. Backend processes request
4. NotificationService sends SignalR message
5. Frontend receives notification via SignalR
6. UI updates with notification data

## Benefits of React-based Testing

- **Integrated Experience**: Works within the main application
- **Real-time Feedback**: Immediate notification reception
- **Professional UI**: Consistent with application design
- **Easy Access**: Available via navigation menu
- **Debugging Tools**: Built-in connection monitoring

## Next Steps

- Add more notification types (project updates, user mentions, etc.)
- Implement notification filtering and search
- Add bulk notification testing
- Create notification templates
- Implement notification persistence and history
