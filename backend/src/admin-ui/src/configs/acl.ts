import {AbilityBuilder, Ability} from '@casl/ability'

export type Subject = string
export type Actions = 'manage' | 'create' | 'read' | 'update' | 'delete'

export type AppAbility = Ability<[Actions, Subject]> | undefined

export const AppAbility = Ability as any
export type ACLObj = {
  action: Actions
  subject: string
}

/**
 * Please define your own Ability rules according to your app requirements.
 * We have just shown Admin and Client rules for demo purpose where
 * admin can manage everything and client can just visit ACL page
 */
const defineRulesFor = (roles: string[], subject: string) => {
  const {can, rules} = new AbilityBuilder(AppAbility)

  //debugger;
  if (roles.includes('admin')) {
    can('manage', 'all')
  } else if (roles.includes('client')) {
    can(['read'], 'acl-page')
  } else {
    can(['read', 'create', 'update', 'delete'], subject)
  }

  return rules
}

export const buildAbilityFor = (roles: string[], subject: string): AppAbility => {
  return new AppAbility(defineRulesFor(roles, subject), {
    // https://casl.js.org/v5/en/guide/subject-type-detection
    // @ts-ignore
    detectSubjectType: object => object!.type
  })
}

export const defaultACLObj: ACLObj = {
  action: 'manage',
  subject: 'all'
}

export default defineRulesFor
