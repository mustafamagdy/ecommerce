import {createSlice} from '@reduxjs/toolkit'
import {fetchData} from "./apis";
import {InitialState} from "src/types/apps/generics";
import {SubscriptionType} from "src/types/apps/subscriptionTypes";

const demoData = new Array(10)
  .fill(0)
  .map((_, i) => ({
    id: (i + 1).toString(),
    name: `Tenant $i}`,
    default: true,
    validForDays: 30,
    price: 99,
    features: []
  }));

const initialState: InitialState<SubscriptionType> = {
  data: demoData,
  total: 1,
  params: {},
  allData: []
};

export const appUsersSlice = createSlice(
  {
    name: 'subscriptions',
    initialState,
    reducers: {},
    extraReducers: builder => {
      builder.addCase(fetchData.fulfilled, (state, action) => {
        state.data = action.payload.users
        state.total = action.payload.total
        state.params = action.payload.params
        state.allData = action.payload.allData
      })
    }
  })

export default appUsersSlice.reducer
