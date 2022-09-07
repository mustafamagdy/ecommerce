Tasks:

========================== API ==========================

[]- Orders:
    [x] Search (order number, customer name, phone number, customer id)
        - Cancel payment, all payments (separate cancel cash permission)
            [x] Create with full customer
            [x] Create cash
            [x] Create with preexisting custer
[] Customers:
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

========================== Reports ===========================
- Export option for:
  - Products
  - Services
  - Service Catalog
 
- Operations reports:
  [x] Orders (by user, service catalog item, payment method, days of week, cash register, branch)
  - VAT report (vat per order with total)
- 
    
========================== Mobile App =========================

This is a mobile app for the customers to order their cloths to be collected, cleaned, and serviced

- ...

========================== FRAMEWORK ==========================

[x] refactor the order number generator to use database instead of files, as the backend will be hosted on multiple
servers with LB
[x] refactor invoice to generate the qrcode, barcode, and logo

- Order api
  [x] can send multiple payments in the same endpoint
    - BUG: if order with customer call failed, a customer will be added anyway, do validation first
    - cancel payment/all payments for order
- Cash-register operations
    - Transfer (with recipient approval)
    - Open/Close?
- Demo account (with reset)
- Permissions
  [x] call an endpoint with higher permission for 1 time operation
    [x] login as admin for another tenant (open new private window and pass jwt token)
    - permission can be scoped on specific data (like one cash register => all invoices to that cash register, or multiple
      subscriptions => can only manage those)
- Predefined roles
- Notification
    - SMS
    - Whatsapp
[x] smart enum (specially for subscription types)
- Pdfs
    - Payment receipt
    - demo watermark
    - serialize reports to xml (NO NEED, we can store the configuration only - done)
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
[X] key, name, main?
- any payment to or from go through a cash register
[X] available balance to to pay from is current balance - total held transfer from
transfer operation works in two steps
[X] create operation (which will hold the transferred amount until the recipient accepts it)

/////// Printing ///////

- we have 3 components:
[x] data 
  [x] come from the application logic (the dto)
[x] template
    [x] come from db based on the current tenant configuration for the template
    [x] template takes the data and construct in memory structure representation of the printable
    [x] we can have multiple templates for a given printable, but only on is active (more than one or none throws an
      exception)
        [x] each template has predefined sections, you cannot add, or delete sections
        [x] you can enable/disable section
        [x] each section has: type (readonly), position, alignment, order, show-debug, bindingValue, valueFormat
        [x] based on some types section can have: font-size, font-name, width, height
        [x] bindingValue:
          [x] propName, propName.propName, propName[].propName
  [x] pdf generator
      [x] uses template to draw the pdf
[x] here how it should work -> 
  [x] export func need to get the active template from that template types (the active invoice template for ex)
  [x] export func also has the model (the dto), it passes the dto to the template to bind its values and get back a bound template
  [x] then the output bound template should contains each section with its properties, and its content has been populated
  [x] export func should pass this bound template to the pdf generator to draw the pdf
- formats
  - format number, date, 

/////// Remaining Tasks ///////

[x] pay for order
[x] cancel order
- update order (add, remove item(s))
- save new order prints the receipt with it
- cancel payment 
- credit note
- print credit note on save
- service catalog categories
- add group with service catalog items
[x] add product, service, to form a service catalog item
- cash register type (cash register, bank account)
[x] cash register manager (what is the permissions)
[x] cash register branch 
- cash register operations (description, debut, credit)
[x] customer list with remaining balance
[x] search customer with balance range
[x] list customer orders + print + totals
- arabic role + permissions
- add role + permissions 
- permission group (for screens)
- add/remote user to/from role
- branch crud
- activate/deactivate branch (deactivate all operations => filter)
- subscription on branch level? how many branches
[x] subscription types (what type of subscriptions? subscription allowed features)
- subscription invoices + payments
- subscription payment goes through main cash register also 
- update tenant info (full page details)
[x] subscription support manager 
- subscription credit note
- subscription print
- subscription send invoice by email, notifications 
- reset train subscription every week

////////////////////////////////////////////////////////////////

subscription types:
    each subscription has some features, but what feature is really mean? -> is it some permissions? no features are 
    some limits, like how many branches tenant can have, how many active users, how many orders per day ...
    but how can we implement that? so each endpoint can request to validate subscription feature before being called
    like create new branch, if user tries to call that api endpoint, and the number branches have reached its subscription
    limit, the system generates an error that your subscription doesn't support that.
    However, each limit need to have its special cases, and its special response. we cannot generalize the limit feature
    in general, but we can as the subscription validator to check if the limit reached or not and what is the output response 

basic feature validator -> takes subscription, the action need to be executed (defined in the endpoint)
                        -> it validates for the current subscription if that met or no

Subscription 
    Feature 1 (yes/no)
    Feature 2 (limit of number of records)


////////////////////////////////////////////////////////////////
- test refactor with better more organized way
     