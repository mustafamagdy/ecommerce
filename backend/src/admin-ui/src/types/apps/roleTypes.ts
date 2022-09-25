// ** Types
// import {ACTION_AUTH_SIGNOUT} from "@aws-amplify/ui-react/dist/types/hooks/actions/constants";
import {ThemeColor} from 'src/@core/layouts/types'

export type RoleLayoutType = {
  id: string | undefined
}

export interface AbilityType {
  resource: string,
  actions: string[]
}

export interface RoleType {
  id: string
  name: string,
  abilities: AbilityType[]
}

