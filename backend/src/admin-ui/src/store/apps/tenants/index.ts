import {createSlice} from '@reduxjs/toolkit'
import {fetchData} from "./apis";
import {InitialState} from "src/types/apps/generics";
import {RoleType} from "src/types/apps/roleTypes";
import {TenantType} from "../../../types/apps/tenantTypes";

const demoData = new Array(10)
  .fill(0)
  .map((_, i) => ({
    id: (i + 1).toString(),
    name: `Tenant $i}`,
    adminEmail: `admin${i}@email.com`,
    phoneNumber: `1234`,
    vatNo: `1234`,
    email: `email@tenant${i}.com`,
    address: `address`,
    adminName: `admin for tenant`,
    adminPhoneNumber: `1234`,
    techSupportUserId: `id`,
    techSupportName: `support name`,
  }));

const initialState: InitialState<TenantType> = {
  data: demoData,
  total: 1,
  params: {},
  allData: []
};

export const appUsersSlice = createSlice(
  {
    name: 'tenants',
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
