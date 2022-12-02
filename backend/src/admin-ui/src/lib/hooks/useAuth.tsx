import { useContext } from 'react'
import { AuthContext } from 'src/lib/context/AuthContext'

export const useAuth = () => useContext(AuthContext)
