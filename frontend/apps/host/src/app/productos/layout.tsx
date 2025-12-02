import { DashboardLayout } from '../../components/DashboardLayout';
import { AuthGuard } from '../../components/AuthGuard';

export default function ProductosLayout({ children }: { children: React.ReactNode }) {
  return (
    <AuthGuard>
      <DashboardLayout>{children}</DashboardLayout>
    </AuthGuard>
  );
}
