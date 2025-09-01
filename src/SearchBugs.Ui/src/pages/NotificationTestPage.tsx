import React from "react";
import NotificationTester from "../components/NotificationTester";

const NotificationTestPage: React.FC = () => {
  return (
    <div className="min-h-screen bg-background">
      <div className="container mx-auto py-8">
        <NotificationTester />
      </div>
    </div>
  );
};

export default NotificationTestPage;
