import {AbilityType} from "./roleTypes";

export type ErrCallbackType = (err: { [key: string]: string }) => void

export type LoginParams = {
  email: string
  password: string
}

export type UserDataType = {
  id: number
  roles: string[]
  email: string
  fullName: string
  username: string
  avatar?: string | null
  tenant: 'root'
}

export type AuthValuesType = {
  loading: boolean
  setLoading: (value: boolean) => void
  logout: () => void
  isInitialized: boolean
  user: UserDataType | null
  setUser: (value: UserDataType | null) => void
  setIsInitialized: (value: boolean) => void
  login: (params: LoginParams, errorCallback?: ErrCallbackType) => void,
  abilities: AbilityType[] | null
  setAbilities: (abilities: AbilityType[] | null) => void;
}
