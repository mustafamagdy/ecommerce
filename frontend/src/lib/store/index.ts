// ** Toolkit imports
import {configureStore} from '@reduxjs/toolkit'

// ** Reducers
import home from 'src/lib/store/apps/home'
import users from 'src/lib/store/apps/users'
import permissions from 'src/lib/store/apps/permissions'
import invoices from 'src/lib/store/apps/invoices'
import tenants from 'src/lib/store/apps/tenants'
import subscriptions from 'src/lib/store/apps/subscriptions'

export const store = configureStore({
  reducer: {
    home,
    users,
    permissions,
    tenants,
    subscriptions,
    invoices,
  },
  middleware: getDefaultMiddleware =>
    getDefaultMiddleware({
      serializableCheck: false
    })
})

export type AppDispatch = typeof store.dispatch
export type RootState = ReturnType<typeof store.getState>
