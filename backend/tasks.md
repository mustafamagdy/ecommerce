Tasks:

========================== API ==========================
- Orders:
  [✅] Search (order number, customer name, phone number, customer id)
  - Cancel payment, all payments (separate cancel cash permission)
  [✅] Create with full customer
  [✅] Create cash
  [✅] Create with preexisting custer
- Customers:
  - Search (name, phone)
  - Auto complete customer
- Service catalog:
  [✅] Get service catalogs
  [✅] Services
  [✅] Items
  - Price list
  - activate/deactivate service item in a price list
- Cash register
  - Search (search transactions)
  - Add/Update/Delete (Name, isMain, Color, Branch)
  - Get basic
  - Get with balance
  - Close (zero balance and transfer to main)
  - open/close history
  - Transfer (with hold until recipient accept)
- Users:
  - predefined roles (activate/deactivate cannot be deleted)
  - Role
  - Users (admin cannot be deleted, activate/deactivate user)
  - User roles
  - Logout all users once role permissions updated
- Branches:
  - Search
  - Add/Update
  - Activate/Deactivate (deactivate all cash register in a branch)
- Tenants:
  - Search (name, phone number, subscription from to, status active/not-active/all, balance from to)
  - Create, renew subscription 
    - (name, vat no, logo, phone number, email, address, zipcode, admin name, admin email, admin phone number)
    - Current subscription (name, date of subscription)
    - Subscription history
    - Technical support
  - Demo account
  - Reset admin account
  - activate/deactivate
  - Invoices (payments, balances)
    - number, date, subscription from to, package, amount, paid, remaining
    - send reminder for payment
    - print invoice, print history

========================== FRAMEWORK ==========================

[x] refactor the order number generator to use database instead of files, as the backend will be hosted on multiple servers with LB
[x] refactor invoice to generate the qrcode, brcode, and logo
- OrdeBr api
  [x] can send multiple payments in the same endpoint
    - BUG: if order with customer call failed, a customer will be added anyway, do validation first
    - cancel payment/all payments for order
- Cash-box operations
    - Transfer (with recipient approval)
    - Open/Close?
- Demo account (with reset)
- Permissions
  [x] call an endpoint with higher permission for 1 time operation
    - login as admin for another tenant (open new private window and pass jwt token)
    - permission can be scoped on specific data (like one cash box => all invoices to that cash box, or multiple subscriptions => can only manage those)
- Predefined roles
- Notification
    - SMS
    - Whatsapp
- Pdfs
    - Payment receipt
    - demo watermark
- Subscriptions
    - ending notification
        - Tenant
        - Root
    - features toggle
    - Payments
    - Branches !

/////// Permissions ///////
1- user 1 has add permission only
2- user want to cancel order but don't have permission
3- supervisor login temporary login for a specific scope (endpoint, serialized parameter)
    - if the supervisor has no permission over this api => 403
    - 
4- user 1 call same api again with the additional one time use manager override token
5- (optional) store the mot in cash for expired token until its ttl end
//////////////////////////