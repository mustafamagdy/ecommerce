Tasks:
[x] refactor the order number generator to use database instead of files, as the backend will be hosted on multiple servers with LB
[] refactor invoice to generate the qrcode, brcode, and logo
- Subscription ending notification
  - Tenant
  - Root
- Subscription features toggle
- Demo account (with reset)
- Login as admin for another tenant (open new private window and pass jwt token)
- Cash-box operations
  - Transfer
  - Open/Close?
- Predefined roles
- Notification
  - SMS
  - Whatsapp



==========================


root tenant get list of almost ending tenant subs and send them an email and record that enal