// ** Toolkit imports
import {configureStore} from '@reduxjs/toolkit'

// ** Reducers
import home from 'src/store/apps/home'
import user from 'src/store/apps/user'
import permissions from 'src/store/apps/permissions'
import invoice from 'src/store/apps/invoice'

export const store = configureStore({
  reducer: {
    home,
    user,
    permissions,
    invoice,
  },
  middleware: getDefaultMiddleware =>
    getDefaultMiddleware({
      serializableCheck: false
    })
})

export type AppDispatch = typeof store.dispatch
export type RootState = ReturnType<typeof store.getState>
