'use client';

import { AuthGuard } from '../../components/AuthGuard';
import { DashboardLayout } from '../../components/DashboardLayout';

export default function ClienteLayout({ children }: { children: React.ReactNode }) {
  return (
    <AuthGuard>
      <DashboardLayout>{children}</DashboardLayout>
    </AuthGuard>
  );
}
