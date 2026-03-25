import appJson from './app.json';

export default {
  ...appJson.expo,
  extra: {
    ...appJson.expo.extra,
    apiUrl: process.env['API_URL'] ?? appJson.expo.extra.apiUrl,
  },
};
