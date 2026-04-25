interface JwtPayload {
  sub?: string
  role?: string
  // .NET long-form claim
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string
  [key: string]: unknown
}

export function decodeJwt(token: string): JwtPayload {
  const part = token.split('.')[1]
  const json = atob(part.replace(/-/g, '+').replace(/_/g, '/'))
  return JSON.parse(json) as JwtPayload
}

export function getRoleFromToken(token: string): string | null {
  const payload = decodeJwt(token)
  return (
    payload.role ??
    payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ??
    null
  )
}

export function getUserIdFromToken(token: string): string | null {
  const payload = decodeJwt(token)
  return payload.sub ?? null
}
