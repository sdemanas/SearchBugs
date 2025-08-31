# Admin Banner Enhanced Features

## Overview

The AdminBanner component has been enhanced with a hidden audit log feature and improved visual design.

## New Features

### 🎨 Improved Color Scheme

- **Changed from purple to slate/blue theme** for a more professional look
- Updated all color classes:
  - Background: `from-slate-50 to-blue-50` with `border-slate-300`
  - Text: `text-slate-800` for titles, `text-slate-600` for descriptions
  - Buttons: `border-slate-400` with `hover:bg-slate-100`
  - Search results: `hover:bg-blue-50` with `text-blue-600` icons

### 🔍 Hidden Audit Log Feature

- **Secret Access**: Click the crown icon 5 times quickly to reveal/hide the audit logs panel
- **Visual Feedback**: Crown has a subtle hover scale animation and tooltip showing remaining clicks
- **Auto-reset**: Click counter resets after 5 seconds of inactivity
- **Real-time Data**: Audit logs refresh every 10 seconds when visible
- **Compact View**: Shows the 5 most recent audit logs with status badges, request names, user info, and timestamps

### 🔐 Security Features

- **Role-based Access**: Only visible to admin users
- **Hidden by Default**: Audit logs panel is completely hidden until activated
- **Activity Monitoring**: Shows system activity with success/failure indicators
- **Quick Access**: Direct link to full audit logs page opens in new tab

## Usage

### For Administrators

1. **Basic Usage**: The banner appears automatically for admin users
2. **Quick Impersonate**: Use the search functionality to find and impersonate users
3. **Secret Audit Access**:
   - Click the crown icon 5 times rapidly
   - The audit logs panel will slide down below the main controls
   - Click the "Hide Audit" button or click crown 5 times again to hide

### For Developers

- The component uses React Query for data fetching
- State management with `useState` for UI toggles
- `useEffect` for automatic cleanup of secret access attempts
- Responsive design with proper z-index for dropdowns

## Technical Implementation

### State Variables

```typescript
const [showAuditLogs, setShowAuditLogs] = useState(false);
const [secretClickCount, setSecretClickCount] = useState(0);
```

### Secret Access Logic

```typescript
const handleSecretAccess = () => {
  const newCount = secretClickCount + 1;
  setSecretClickCount(newCount);

  if (newCount >= 5) {
    setShowAuditLogs(!showAuditLogs);
    setSecretClickCount(0);
    // Show toast notification
  }
};
```

### Data Fetching

- **Users Search**: Triggered when typing in impersonation search (min 2 characters)
- **Audit Logs**: Only fetched when the hidden panel is visible
- **Auto-refresh**: Audit logs refresh every 10 seconds when visible

## UI Components Used

- `Badge`: For success/failure status indicators
- `Button`: Various action buttons with consistent styling
- `Card/CardContent`: Main container with gradient background
- `Input`: Search functionality with icon
- `useToast`: User feedback for actions
- Lucide React icons: Crown, Shield, Activity, Clock, etc.

## Color Palette

- **Primary**: Slate tones (slate-50, slate-300, slate-600, slate-700, slate-800)
- **Accent**: Blue tones (blue-50, blue-600)
- **Success**: Default badge (green)
- **Error**: Destructive badge (red)
- **Warning**: Amber (amber-500, amber-700)
- **Audit Access**: Orange/Red tones (orange-600, red-400, red-700)

This enhanced admin banner provides a clean, professional interface with hidden power-user features for system monitoring and administration.
