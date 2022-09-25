// ** Types
// import {ACTION_AUTH_SIGNOUT} from "@aws-amplify/ui-react/dist/types/hooks/actions/constants";
import {ThemeColor} from 'src/@core/layouts/types'

export type UserLayoutType = {
  id: string | undefined
}

export interface UserType {
  id: string
  firstName: string
  lastName: string
  email: string
  active: boolean
  role: string
  userName: string
  emailConfirmed: boolean
  phoneNumber: string
  imagePath: string
  status: string | 'active' | 'pending' | 'inactive'
  fullName: string;
}

