import { Footer } from "@/components/footer/Footer";
import { Header } from "@/components/header/Header";
import { ImpersonationBanner } from "@/components/ImpersonationBanner";
import { AdminBanner } from "@/components/AdminBanner";
import { Outlet } from "react-router-dom";
import { useAuth } from "@/hooks/useAuth";

export default function Layout() {
  const { impersonate } = useAuth();

  const handleQuickImpersonate = async (userId: string) => {
    try {
      await impersonate(userId);
    } catch (error) {
      console.error("Quick impersonation failed:", error);
    }
  };

  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <div className="container mx-auto px-4">
        <AdminBanner onQuickImpersonate={handleQuickImpersonate} />
        <ImpersonationBanner />
      </div>
      <main className="flex-1 bg-gray-100">
        <Outlet />
      </main>
      <Footer />
    </div>
  );
}
