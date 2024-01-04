interface Endpoint {
  url: string,
  anonymous?: boolean
}

export const isAnonymous = (path: string): boolean => {
  const key = (Object.keys(endPoints) as (keyof typeof endPoints)[]).find(a => endPoints[a].url === path)!;
  return endPoints[key].anonymous || false
}

export const endPoints: Record<string, Endpoint> = {
  // ** Auth
  abilities: {url: '/api/roles/abilities', anonymous: true},
  meEndpoint: {url: '/api/personal/profile'},
  loginEndpoint: {url: '/api/tokens'},

  // ** Users
  userList: {url: '/api/Users/search'},
  userCreate: {url: '/api/Users'},
}
