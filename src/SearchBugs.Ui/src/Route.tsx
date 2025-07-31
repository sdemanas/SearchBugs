import { createBrowserRouter, RouterProvider } from "react-router-dom";
import {
  BugAddPage,
  BugDetailsPage,
  BugsPage,
  CreateProjectPage,
  DashboardPage,
  LoginPage,
  NotificationsPage,
  ProfilePage,
  ProjectDetailsPage,
  ProjectsPage,
  RegisterPage,
  RepositoryPage,
  RepositoryDetailsPage,
  SettingPage,
  UserDetailsPage,
  UsersPage,
} from "./pages";
import MainLayout from "./layouts/Main";

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
    element: <MainLayout />,
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
        element: <ProfilePage />,
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
        path: "/:username/:repository",
        element: <RepositoryPage />,
        handle: {
          title: "Repository",
          icon: "GitBranch",
        },
      },
      {
        path: "/repositories/:url",
        element: <RepositoryDetailsPage />,
        handle: {
          title: "Repository Details",
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
    ],
  },
]);

const Route = () => {
  return <RouterProvider router={router} />;
};

export default Route;
