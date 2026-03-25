const API_BASE = 'https://163.176.217.87.sslip.io';

/**
 * Cloudflare Pages Function — proxies all /api/* requests to the backend.
 *
 * The _redirects status-200 proxy only supports GET; using a Pages Function
 * allows POST, PATCH, DELETE, etc. to be forwarded correctly.
 *
 * Route: /api/[[path]] matches /api/<anything> and passes :path to the backend.
 */
export async function onRequest(context) {
  const { request } = context;
  const url = new URL(request.url);

  // Strip the /api prefix to get the backend path
  const backendPath = url.pathname.slice('/api'.length);
  const targetUrl = `${API_BASE}${backendPath}${url.search}`;

  return fetch(new Request(targetUrl, request));
}
