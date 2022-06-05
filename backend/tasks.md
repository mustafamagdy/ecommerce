Tasks:
[x] refactor the order number generator to use database instead of files, as the backend will be hosted on multiple servers with LB
[x] refactor invoice to generate the qrcode, brcode, and logo
- OrdeBr api
  - can send multiple payments in the same endpoint
- login as admin for some tenant
- call an endpoint with higher permission for 1 time operation
- cancel payment/all payments for order
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



==========================


root tenant get list of almost ending tenant subs and send them an email and record that enal