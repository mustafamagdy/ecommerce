// ** JWT import
import jwt from 'jsonwebtoken'

// ** Mock Adapter
import mock from 'src/@fake-db/mock'

// ** Config
import authConfig from 'src/configs/auth'
import {UserDataType} from "src/types/apps/auth";


const users: UserDataType[] = [
  {
    id: 1,
    role: 'admin',
    fullName: 'John Doe',
    username: 'johndoe',
    email: 'admin@root.com',
    tenant: 'root'
  },
  {
    id: 2,
    role: 'client',
    fullName: 'Jane Doe',
    username: 'janedoe',
    email: 'client@root.com',
    tenant: 'root'
  }
]

// ! These two secrets should be in .env file and not in any other file
const jwtConfig = {
  secret: 'dd5f3089-40c3-403d-af14-d0c228b05cb4',
  refreshTokenSecret: '7c4c1c50-3230-45bf-9eae-c9b2e401c767'
}

mock.onPost(authConfig.loginEndpoint).reply(request => {
  const {email, password} = JSON.parse(request.data)

  let error = {
    email: ['Something went wrong']
  }

  const user = users.find(u => u.email === email)

  if (user) {
    const token = jwt.sign({id: user.id}, jwtConfig.secret)
    const response = {
      [authConfig.storageTokenKeyName]: token,
      [authConfig.storageRefreshTokenKeyName]: undefined,
      [authConfig.storageRefreshTokenExpiryDateKeyName]: undefined
    }

    return [200, response]
  } else {
    error = {
      email: ['email or Password is Invalid']
    }

    return [400, {error}]
  }
})


mock.onGet(authConfig.meEndpoint).reply(config => {
  // @ts-ignore
  const token = config.headers.Authorization as string

  // get the decoded payload and header
  const decoded = jwt.decode(token, {complete: true})

  if (decoded) {
    // @ts-ignore
    const {id: userId} = decoded.payload

    const userData = JSON.parse(JSON.stringify(users.find((u: UserDataType) => u.id === userId)))

    delete userData.password

    return [200, {userData}]
  } else {
    return [401, {error: {error: 'Invalid User'}}]
  }
})
