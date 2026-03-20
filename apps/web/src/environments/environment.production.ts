export const environment = {
  production: true,
  apiUrl: '/api',
  googleClientId: '545148539649-ki1aq0qco73mj19umbl1mrgc8lta2f87.apps.googleusercontent.com',
  // Must match the redirect URI registered in Google Cloud Console exactly.
  // Cloudflare Pages generates per-deploy preview URLs (e.g. <hash>.medcontrol-web.pages.dev)
  // that are NOT registered, so we pin the canonical production origin here.
  googleRedirectUri: 'https://medcontrol-web.pages.dev' as string | null,
};
