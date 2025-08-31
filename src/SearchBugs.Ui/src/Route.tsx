import { createBrowserRouter, RouterProvider } from "react-router-dom";
import {
  BugAddPage,
  BugDetailsPage,
  BugsPage,
  CreateProjectPage,
  CreateRepositoryPage,
  DashboardPage,
  LoginPage,
  NotificationsPage,
  ProjectDetailsPage,
  ProjectsPage,
  RegisterPage,
  RepositoryPage,
  RolePermissionsPage,
  SettingPage,
  UserDetailsPage,
  UsersPage,
} from "./pages";
import AuditLogsPage from "./pages/AuditLogsPage";
import { RepositoryListPage } from "./modules/repository/RepositoryListPage";
import { EnhancedProfilePage } from "./pages/EnhancedProfilePage";
import NotificationTestPage from "./pages/NotificationTestPage";
import MainLayout from "./layouts/Main";
import { ProtectedRoute } from "./components/ProtectedRoute";

const router = createBrowserRouter([
  {
    path: "/login",
    element: <LoginPage />,
    handle: {
      title: "Login",
      hideNav: true,
    },
  },
  {
    path: "/register",
    element: <RegisterPage />,
    handle: {
      title: "Register",
      hideNav: true,
    },
  },
  {
    element: (
      <ProtectedRoute>
        <MainLayout />
      </ProtectedRoute>
    ),
    handle: {
      title: "SearchBugs",
    },
    children: [
      {
        path: "/",
        element: <DashboardPage />,
        handle: {
          title: "Dashboard",
          icon: "LayoutDashboard",
        },
      },
      {
        path: "/projects",
        element: <ProjectsPage />,
        handle: {
          title: "Projects",
          icon: "FolderKanban",
        },
      },
      {
        path: "/projects/create",
        element: <CreateProjectPage />,
        handle: {
          title: "Create Project",
          icon: "PlusCircle",
        },
      },
      {
        path: "/projects/:projectId",
        element: <ProjectDetailsPage />,
        handle: {
          title: "Project Details",
          icon: "FolderKanban",
        },
      },
      {
        path: "/users",
        element: <UsersPage />,
        handle: {
          title: "Users",
          icon: "Users",
          adminOnly: true,
        },
      },
      {
        path: "/profile",
        element: <EnhancedProfilePage />,
        handle: {
          title: "Profile",
          icon: "User",
        },
      },
      {
        path: "/users/:userId",
        element: <UserDetailsPage />,
        handle: {
          title: "User Details",
          icon: "User",
          adminOnly: true,
        },
      },
      {
        path: "/roles-permissions",
        element: <RolePermissionsPage />,
        handle: {
          title: "Role Permissions",
          icon: "Shield",
          adminOnly: true,
        },
      },
      {
        path: "/audit-logs",
        element: <AuditLogsPage />,
        handle: {
          title: "Audit Logs",
          icon: "FileText",
          adminOnly: true,
        },
      },
      {
        path: "/repositories",
        element: <RepositoryListPage />,
        handle: {
          title: "Repositories",
          icon: "GitBranch",
        },
      },
      {
        path: "/repositories/create",
        element: <CreateRepositoryPage />,
        handle: {
          title: "Create Repository",
          icon: "GitBranch",
        },
      },
      {
        path: "/repositories/:url",
        element: <RepositoryPage />,
        handle: {
          title: "Repository",
          icon: "GitBranch",
        },
      },
      {
        path: "/bugs",
        element: <BugsPage />,
        handle: {
          title: "Bugs",
          icon: "Bug",
        },
      },
      {
        path: "/add-bug",
        element: <BugAddPage />,
        handle: {
          title: "Add Bug",
          icon: "PlusCircle",
        },
      },
      {
        path: "/projects/:projectId/add-bug",
        element: <BugAddPage />,
        handle: {
          title: "Add Bug",
          icon: "PlusCircle",
        },
      },
      {
        path: "/bugs/:bugId",
        element: <BugDetailsPage />,
        handle: {
          title: "Bug Details",
          icon: "Bug",
        },
      },
      {
        path: "/notifications",
        element: <NotificationsPage />,
        handle: {
          title: "Notifications",
          icon: "Bell",
        },
      },
      {
        path: "/settings",
        element: <SettingPage />,
        handle: {
          title: "Settings",
          icon: "Settings",
        },
      },
      {
        path: "/test-notifications",
        element: <NotificationTestPage />,
        handle: {
          title: "Notification Test",
          icon: "TestTube",
        },
      },
    ],
  },
]);

const Route = () => {
  return <RouterProvider router={router} />;
};

export default Route;
