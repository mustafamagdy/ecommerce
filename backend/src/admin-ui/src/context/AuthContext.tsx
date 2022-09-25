// ** React Imports
import {createContext, useEffect, useState, ReactNode} from 'react'

// ** Next Import
import {useRouter} from 'next/router'

// ** Axios
import axios from 'axios'

// ** Config
import authConfig from 'src/configs/auth'

// ** Types
import {AuthValuesType, LoginParams, ErrCallbackType, UserDataType} from 'src/types/apps/auth'
import {AbilityType} from "../types/apps/roleTypes";

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

        let _ = await axios.get(authConfig.abilities, {
          headers: {tenant: 'root'}
        });
        setAbilities({..._.data});

        // ** Set user token
        const storedToken = window.localStorage.getItem(authConfig.storageTokenKeyName)!
        if (storedToken) {
          _ = await axios.get(authConfig.meEndpoint, {
            headers: {
              Authorization: storedToken,
              tenant: 'root'
            }
          });
          setUser({..._.data.userData})
        } else {
          setLoading(false)
        }

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
    axios
      .post(authConfig.loginEndpoint, params)
      .then(async res => {
        window.localStorage.setItem(authConfig.storageTokenKeyName, res.data.token)
        window.localStorage.setItem(authConfig.storageRefreshTokenKeyName, res.data.refreshToken)
        window.localStorage.setItem(authConfig.storageRefreshTokenExpiryDateKeyName, res.data.refreshTokenExpiryTime)
      })
      .then(() => {
        axios
          .get(authConfig.meEndpoint, {
            headers: {
              Authorization: window.localStorage.getItem(authConfig.storageTokenKeyName)!
            }
          })
          .then(async response => {
            const returnUrl = router.query.returnUrl

            setUser({...response.data.userData})
            await window.localStorage.setItem('userData', JSON.stringify(response.data.userData))
            const redirectURL = returnUrl && returnUrl !== '/' ? returnUrl : '/'
            await router.replace(redirectURL as string)
          })
      })
      .catch(err => {
        if (errorCallback) errorCallback(err)
      })
  }

  const clearUserData = () => {
    window.localStorage.removeItem('userData')
    window.localStorage.removeItem(authConfig.storageTokenKeyName)
    window.localStorage.removeItem(authConfig.storageRefreshTokenKeyName)
    window.localStorage.removeItem(authConfig.storageRefreshTokenExpiryDateKeyName)
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
