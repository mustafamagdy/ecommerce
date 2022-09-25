interface Endpoint {
  url: string,
  anonymous?: boolean
}

export const endPoints: Record<string, Endpoint> = {
  abilities: {url: '/roles/abilities'},
  meEndpoint: {url: '/tokens/me'},
  loginEndpoint: {url: '/tokens'},
  storageTokenKeyName: {url: 'token'},
  storageRefreshTokenKeyName: {url: 'refreshToken'},
  storageRefreshTokenExpiryDateKeyName: {url: 'refreshTokenExpiryTime'},
}
