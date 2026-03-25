import type { ExpoConfig } from 'expo/config';
import appJson from './app.json';

const config: ExpoConfig = {
  ...appJson.expo,
  extra: {
    ...appJson.expo.extra,
    apiUrl: process.env['API_URL'] ?? appJson.expo.extra.apiUrl,
  },
};

export default config;
