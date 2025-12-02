import { DashboardLayout } from '../../components/DashboardLayout';
import { AuthGuard } from '../../components/AuthGuard';

export default function LotesLayout({ children }: { children: React.ReactNode }) {
  return (
    <AuthGuard>
      <DashboardLayout>{children}</DashboardLayout>
    </AuthGuard>
  );
}
