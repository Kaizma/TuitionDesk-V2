import { createBrowserRouter, Navigate } from "react-router-dom";
import { AppShell } from "./AppShell";
import { AboutPage } from "./pages/AboutPage";
import { NotFoundPage } from "./pages/NotFoundPage";
import { RouteErrorPage } from "./pages/RouteErrorPage";
import { ClassDetailPage } from "../features/classes/pages/ClassDetailPage";
import { ClassFormPage } from "../features/classes/pages/ClassFormPage";
import { ClassesListPage } from "../features/classes/pages/ClassesListPage";
import { StudentDetailPage } from "../features/students/pages/StudentDetailPage";
import { StudentFormPage } from "../features/students/pages/StudentFormPage";
import { StudentsListPage } from "../features/students/pages/StudentsListPage";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <AppShell />,
    errorElement: <RouteErrorPage />,
    children: [
      { index: true, element: <Navigate to="/students" replace /> },
      { path: "students", element: <StudentsListPage /> },
      { path: "students/new", element: <StudentFormPage /> },
      { path: "students/:id", element: <StudentDetailPage /> },
      { path: "students/:id/edit", element: <StudentFormPage /> },
      { path: "classes", element: <ClassesListPage /> },
      { path: "classes/new", element: <ClassFormPage /> },
      { path: "classes/:id", element: <ClassDetailPage /> },
      { path: "classes/:id/edit", element: <ClassFormPage /> },
      { path: "about", element: <AboutPage /> },
      { path: "*", element: <NotFoundPage /> },
    ],
  },
]);
