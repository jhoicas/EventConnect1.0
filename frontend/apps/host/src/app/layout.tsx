import type { Metadata } from 'next';
import { Providers } from '../components/Providers';
import { ErrorBoundary } from '../components/ErrorBoundary';

export const metadata: Metadata = {
  title: 'EventConnect - Sistema de Gestión de Eventos',
  description: 'Plataforma integral para gestión de eventos y alquiler de mobiliario',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="es">
      <body>
        <ErrorBoundary>
          <Providers>{children}</Providers>
        </ErrorBoundary>
      </body>
    </html>
  );
}
