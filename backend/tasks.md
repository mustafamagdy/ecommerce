Tasks:
[x] refactor the order number generator to use database instead of files, as the backend will be hosted on multiple servers with LB
[x] refactor invoice to generate the qrcode, brcode, and logo
- OrdeBr api
  [x] can send multiple payments in the same endpoint
  - BUG: if order with customer call failed, a customer will be added anyway, do validation first
- Permissions
  - call an endpoint with higher permission for 1 time operation
  - login as admin for some tenant
  - cancel payment/all payments for order
  - permission can be scoped on specific data (like one cash box => all invoices to that cash box, or multiple subscriptions => can only manage those)
- Subscriptions
  - ending notification
    - Tenant
    - Root 
  - features toggle
  - Payments
- Demo account (with reset)
- Login as admin for another tenant (open new private window and pass jwt token)
- Cash-box operations
  - Transfer (with recipient approval)
  - Open/Close?
- Predefined roles
- Notification
  - SMS
  - Whatsapp
- Pdfs
  - Payment receipt
  - demo watermark



==========================


root tenant get list of almost ending tenant subs and send them an email and record that email

////// Permissions ///
1- user 1 has add permission only
2- user want to cancel order but don't have permission
3- supervisor login temporary login for a specific scope (endpoint, serialized parameter)
    - if the supervisor has no permission over this api => 403
    - 
4- user 1 call same api again with the additional one time use manager override token
5- store the mot in cash for expired token until its ttl end