module.exports = {
  displayName: 'mobile',
  resolver: require.resolve('./jest.resolver.js'),
  preset: 'jest-expo',
  moduleFileExtensions: ['ts', 'js', 'html', 'tsx', 'jsx'],
  setupFilesAfterEnv: ['<rootDir>/src/test-setup.ts'],
  moduleNameMapper: {
    '\\.svg$': '@nx/expo/plugins/jest/svg-mock',
  },
  transform: {
    '\\.[jt]sx?$': [
      'babel-jest',
      {
        configFile: __dirname + '/.babelrc.js',
      },
    ],
    '^.+\\.(bmp|gif|jpg|jpeg|mp4|png|psd|svg|webp|ttf|otf|m4v|mov|mp4|mpeg|mpg|webm|aac|aiff|caf|m4a|mp3|wav|html|pdf|obj)$':
      require.resolve('jest-expo/src/preset/assetFileTransformer.js'),
  },
  // Permite que pacotes react-native com sintaxe Flow/ESM sejam transformados pelo babel-jest.
  // O prefixo \\.pnpm na negação inclui o virtual store do pnpm para que
  // os pacotes aninhados em node_modules/.pnpm/*/node_modules também sejam transformados.
  transformIgnorePatterns: [
    'node_modules/(?!\\.pnpm|(jest-)?react-native|@react-native(-community)?|expo(nent)?|@expo(nent)?/.*|@expo-google-fonts/.*|react-navigation|@react-navigation/.*|@unimodules/.*|unimodules|sentry-expo|native-base|react-native-svg)',
  ],
  // Exclui App.tsx do coverage — é scaffold gerado pelo nx que será substituído
  // pelas telas reais do MedControl. O threshold de 70% se aplica ao código de produção.
  collectCoverageFrom: [
    'src/**/*.{ts,tsx}',
    '!src/app/App.tsx',
    '!src/**/*.spec.{ts,tsx}',
    '!src/**/*.test.{ts,tsx}',
    '!src/test-setup.ts',
  ],
  coverageDirectory: '../../coverage/apps/mobile',
};
