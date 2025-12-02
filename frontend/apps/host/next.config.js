/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  transpilePackages: ['@eventconnect/ui', '@eventconnect/shared'],
  experimental: {
    optimizePackageImports: ['@chakra-ui/react', 'lucide-react']
  },
  webpack: (config, { isServer }) => {
    if (!isServer) {
      config.optimization.splitChunks = {
        chunks: 'all',
        cacheGroups: {
          default: false,
          vendors: false,
          chakra: {
            name: 'chakra-ui',
            test: /[\\/]node_modules[\\/](@chakra-ui)[\\/]/,
            priority: 40
          },
          emotion: {
            name: 'emotion',
            test: /[\\/]node_modules[\\/](@emotion)[\\/]/,
            priority: 30
          },
          redux: {
            name: 'redux',
            test: /[\\/]node_modules[\\/](@reduxjs|react-redux)[\\/]/,
            priority: 25
          },
          common: {
            name: 'common',
            minChunks: 2,
            priority: 20,
            reuseExistingChunk: true
          }
        }
      };
    }
    return config;
  }
};

module.exports = nextConfig;
