// The JWT itself lives in an httpOnly cookie the backend sets on login — never readable here,
// which is the whole point (see E1: it closes the XSS-token-theft vector localStorage had).
// This cookie is different: it's the CSRF "request token" half of the backend's double-submit
// cookie check (see Program.cs), deliberately non-httpOnly so the SPA can read it and echo it
// back as a header on state-changing requests. It isn't a secret — its value only matters
// because same-origin JS is the only thing that can read it.
export function getCsrfToken(): string | null {
  const match = document.cookie.match(/(?:^|;\s*)XSRF-TOKEN=([^;]+)/)
  return match ? decodeURIComponent(match[1]) : null
}
