export async function apiRequest(
  path: string,
  options: RequestInit = {},
): Promise<Response> {
  return fetch(path, {
    ...options,
    credentials: 'include',
  })
}
