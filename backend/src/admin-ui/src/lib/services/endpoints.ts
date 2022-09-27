interface Endpoint {
  url: string,
  anonymous?: boolean
}

export const isAnonymous = (path: string): boolean => {
  const key = (Object.keys(endPoints) as (keyof typeof endPoints)[]).find(a => endPoints[a].url === path)!;
  return endPoints[key].anonymous || false
}

export const endPoints: Record<string, Endpoint> = {
  abilities: {url: '/roles/abilities', anonymous: true},
  meEndpoint: {url: '/personal/profile'},
  loginEndpoint: {url: '/tokens'},
  storageTokenKeyName: {url: 'token'},
  storageRefreshTokenKeyName: {url: 'refreshToken'},
  storageRefreshTokenExpiryDateKeyName: {url: 'refreshTokenExpiryTime'},
}
