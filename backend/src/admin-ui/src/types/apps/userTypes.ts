// ** Types
import {ThemeColor} from 'src/@core/layouts/types'
import {ACTION_AUTH_SIGNOUT} from "@aws-amplify/ui-react/dist/types/hooks/actions/constants";

export type UserLayoutType = {
  id: string | undefined
}

interface UserType {
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

  get fullName(): string;

  get status(): 'active' | 'pending' | 'inactive';
}


export class AppUser implements UserType {
  constructor(public id: string,
              public firstName: string,
              public lastName: string,
              public email: string,
              public active: boolean,
              public role: string,
              public userName: string,
              public emailConfirmed: boolean,
              public phoneNumber: string,
              public imagePath: string) {
  }

  get fullName(): string {
    return `${this.firstName} ${this.lastName}`;
  }

  get status(): 'active' | 'pending' | 'inactive' {
    if (!this.emailConfirmed) return 'pending';
    else if (!this.active) return 'inactive';
    return 'active';
  }

  avatarColor?: ThemeColor
}

