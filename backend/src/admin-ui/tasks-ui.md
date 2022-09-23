
Navigation menu
- Home -> Dashboard (personalized per user according to permission)
- Users
  - user mgmt
  - roles
- Tenants
  - tenants, subscriptions, payments
  - tenant support (user reset, admin support)
- Subscriptions
  - packages
  - package features, prices,
- System (maintenance, backups, workflow, admin settings, monitoring, updates)



======================== 
- system:
  - users list 
    - active one for edit => [template -> user view page]
      - basic info [template -> user view page ]
      - latest activities (logs/audit)  [template -> user view page -> user activity timeline (recent top 10)]
      - security (reset, lock, unlock) [template -> user view page -> security tab]
  - roles list 
    - active one for edit
      - name, icon [template -> role list]
      - list of permissions/resource [template -> role view & edit role popup]
- tenants:
  - all tenants
  - create new tenant => [template -> dialogs -> create app]
  - active tenant for view/edit 
    - basic info => [template -> user view page]
      - security (reset admin password) [template -> user view page -> security tab]
      - current plan [template -> user view page -> plan section & billing & plan tab]
      - history [template -> user view page -> user activity timeline (recent top 10)]
      - billing history (payments) [template -> user view page -> security tab -> recent invoices section]
    - branches & total sales [template -> invoice list]
    - latest activities (support tickets) [template -> user view page -> user activity timeline (recent top 10)]
    - invoices [template -> user view page -> invoice list & view & edit & print]
    - 

state:
- app:
  - current user => [context]
  - menu, theme, settings => [context]
  - roles/permissions (for acl)
  - dashboard
- system:
  - users list
  - active one for edit
  - roles list
  - active one for edit
  - settings
  - workflows
  - monitoring
- tenants:
  - all tenants
  - active one for edit
  - invoices
  - subscription history
  - payments
  - subscription packages
  - package features
