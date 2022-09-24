// ** Toolkit imports
import {configureStore} from '@reduxjs/toolkit'

// ** Reducers
import home from 'src/store/apps/home'
import users from 'src/store/apps/users'
import permissions from 'src/store/apps/permissions'
import invoices from 'src/store/apps/invoices'
import tenants from 'src/store/apps/tenants'
import subscriptions from 'src/store/apps/subscriptions'

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
