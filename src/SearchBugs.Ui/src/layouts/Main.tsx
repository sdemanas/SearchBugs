import { Footer } from "@/components/footer/Footer";
import { Header } from "@/components/header/Header";
import { ImpersonationBanner } from "@/components/ImpersonationBanner";
import { Outlet } from "react-router-dom";

export default function Layout() {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <div className="container mx-auto px-4">
        <ImpersonationBanner />
      </div>
      <main className="flex-1 bg-gray-100">
        <Outlet />
      </main>
      <Footer />
    </div>
  );
}
