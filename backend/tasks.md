Tasks:

========================== API ==========================

- Orders:
  [x] Search (order number, customer name, phone number, customer id)
    - Cancel payment, all payments (separate cancel cash permission)
      [x] Create with full customer
      [x] Create cash
      [x] Create with preexisting custer
- Customers:
  [x] Search (name, phone)
  [x] Auto complete customer
- Service catalog:
  [x] Get service catalogs
  [x] Services
  [x] Items
    - Price list
    - activate/deactivate service item in a price list
- Cash register
  [x] Search (search transactions)
  [x] Add/Update/Delete (Name, isMain, Color, Branch)
  [x] Get basic
  [x] Get with balance
  [x] Close (zero balance and transfer to main)
  [x] open/close history
  [x] Transfer (with hold until recipient accept)
- Users:
  [x] predefined roles (activate/deactivate cannot be deleted) [I chose to delete instead of activate/deactivate]
  [x] Role
    - Users (admin cannot be deleted, activate/deactivate user)
      [x] User roles
    - Logout all users once role permissions updated
- Branches:
  [x] Search
  [x] Add/Update
    - Activate/Deactivate (deactivate all cash register in a branch)
- Tenants:
  [x] Search (name, phone number, subscription from to, status active/not-active/all, balance from to)
    - Create, renew subscription
      [x] (name, vat no, logo, phone number, email, address, zipcode, admin name, admin email, admin phone number)
      [x] Current subscription (name, date of subscription)
      [x] Subscription history
        - Technical support
    - Demo account
    - Reset admin account
      [x] activate/deactivate
    - Invoices (payments, balances)
        - number, date, subscription from to, package, amount, paid, remaining
        - send reminder for payment
        - print invoice, print history

General

- validation rules need to be common between backend and frontend
  [x] refactor domain models
- update events for all models
- tests

========================== FRAMEWORK ==========================

[x] refactor the order number generator to use database instead of files, as the backend will be hosted on multiple
servers with LB
[x] refactor invoice to generate the qrcode, barcode, and logo

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
    - permission can be scoped on specific data (like one cash box => all invoices to that cash box, or multiple
      subscriptions => can only manage those)
- Predefined roles
- Notification
    - SMS
    - Whatsapp
- smart enum (specially for subscription types)
- Pdfs
    - Payment receipt
    - demo watermark
    - serialize reports to xml
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

Tenant has branches
each branch has cash register(s)

/////// Cash Register ///////

order helper
=> register the payment in cash register
=> cash register service
=> get the current cash register from header (resolver)
=> (todo: validate branch and user)
=> validate cash register is open (attribute)
=> register the payment operation

Cash register
- key, name, main?
- any payment to or from go through a cash register
- available balance to to pay from is current balance - total held transfer from
transfer operation works in two steps
- create operation (which will hold the transferred amount until the recipient accepts it)

/////// Printing ///////

- we have 3 components:
    - data
        - come from the application logic (the dto)
    - template
        - come from db based on the current tenant configuration for the template
        - template takes the data and construct in memory structure representation of the printable
        - we can have multiple templates for a given printable, but only on is active (more than one or none throws an
          exception)
            - each template has predefined sections, you cannot add, or delete sections
            - you can enable/disable section
            - each section has: type (readonly), position, alignment, order, show-debug, bindingValue, valueFormat
            - based on some types section can have: font-size, font-name, width, height
            - bindingValue:
              - propName, propName.propName, propName[].propName
    - pdf generator
        - uses template to draw the pdf
    - here how it should work -> 
      - export func need to get the active template from that template types (the active invoice template for ex)
      - export func also has the model (the dto), it passes the dto to the template to bind its values and get back a bound template
      - then the output bound template should contains each section with its properties, and its content has been populated
      - export func should pass this bound template to the pdf generator to draw the pdf
    - 
