// ** Types
// import {ACTION_AUTH_SIGNOUT} from "@aws-amplify/ui-react/dist/types/hooks/actions/constants";
import {ThemeColor} from 'src/@core/layouts/types'

export interface SubscriptionType {
  id: string
  name: string
  default: boolean
  validForDays: number
  price: number
  features: SubscriptionFeatureType[]
}

export interface SubscriptionFeatureType {
  feature: string,
  value: string
}

