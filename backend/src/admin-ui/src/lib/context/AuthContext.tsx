// ** React Imports
import {createContext, useEffect, useState, ReactNode} from 'react'

// ** Next Import
import {useRouter} from 'next/router'

// ** Config
import authConfig from 'src/configs/auth'
import storage from 'src/lib/services/storage'
import {endPoints} from 'src/lib/services/endpoints'
import {Http} from 'src/lib/services/http'

// ** Types
import {AuthValuesType, LoginParams, ErrCallbackType, UserDataType} from 'src/types/apps/auth'
import {AbilityType} from "src/types/apps/roleTypes";

// ** Defaults
const defaultProvider: AuthValuesType = {
  user: null,
  loading: true,
  setUser: () => null,
  setLoading: () => Boolean,
  isInitialized: false,
  login: () => Promise.resolve(),
  logout: () => Promise.resolve(),
  setIsInitialized: () => Boolean,
  abilities: [],
  setAbilities: () => null
}

const AuthContext = createContext(defaultProvider)

type Props = {
  children: ReactNode
}

const AuthProvider = ({children}: Props) => {
  // ** States
  const [user, setUser] = useState<UserDataType | null>(defaultProvider.user)
  const [abilities, setAbilities] = useState<AbilityType[] | null>(defaultProvider.abilities)
  const [loading, setLoading] = useState<boolean>(defaultProvider.loading)
  const [isInitialized, setIsInitialized] = useState<boolean>(defaultProvider.isInitialized)

  // ** Hooks
  const router = useRouter()

  useEffect(() => {
    const initAuth = async (): Promise<void> => {
      try {
        setIsInitialized(true)
        // ** Get abilities
        setLoading(true)

        const rolesAndAbilities = await Http.get(endPoints.abilities.url);
        debugger
        setAbilities({...rolesAndAbilities});

        // ** Set user token
        const storedToken = storage.getItem(authConfig.storageTokenKeyName)!
        if (storedToken) {
          const userData = await Http.get(endPoints.meEndpoint.url);
          debugger
          setUser({...userData})
        }

        setLoading(false)
      } catch (e) {
        clearUserData()
        setUser(null)
        setAbilities(null)
        setLoading(false)
      }
    }
    initAuth()
  }, [])

  const handleLogin = (params: LoginParams, errorCallback?: ErrCallbackType) => {
    Http
      .post(endPoints.loginEndpoint.url, params)
      .then(async data => {
        storage.setItem(authConfig.storageTokenKeyName, data.token)
        storage.setItem(authConfig.storageRefreshTokenKeyName, data.refreshToken)
        storage.setItem(authConfig.storageRefreshTokenExpiryDateKeyName, data.refreshTokenExpiryTime)
      })
      .then(() => {
        Http
          .get(endPoints.meEndpoint.url)
          .then(async data => {
            const returnUrl = router.query.returnUrl
            setUser({...data})
            await storage.setItem('userData', JSON.stringify(data))
            const redirectURL = returnUrl && returnUrl !== '/' ? returnUrl : '/'
            await router.replace(redirectURL as string)
          })
      })
      .catch(err => {
        if (errorCallback) errorCallback(err)
      })
  }

  const clearUserData = () => {
    storage.removeItem('userData')
    storage.removeItem(authConfig.storageTokenKeyName)
    storage.removeItem(authConfig.storageRefreshTokenKeyName)
    storage.removeItem(authConfig.storageRefreshTokenExpiryDateKeyName)
  }

  const handleLogout = () => {
    setUser(null)
    setIsInitialized(false)
    clearUserData();
    router.push('/login')
  }

  const values = {
    user,
    loading,
    setUser,
    setLoading,
    isInitialized,
    setIsInitialized,
    login: handleLogin,
    logout: handleLogout,
    abilities,
    setAbilities
  }

  return <AuthContext.Provider value={values}>{children}</AuthContext.Provider>
}

export {AuthContext, AuthProvider}
