import {createSlice} from '@reduxjs/toolkit'
import {fetchData} from "./apis";
import {InitialState} from "src/types/apps/generics";
import {RoleType} from "src/types/apps/roleTypes";

const demoData = new Array(10)
  .fill(0)
  .map((_, i) => ({
    id: (i + 1).toString(),
    name: `role ${i}`,
    abilities: [
      {
        resource: 'dashboard',
        actions: ['view']
      }, {
        resource: 'users',
        actions: ['view', 'list']
      }
    ]
  }));

const initialState: InitialState<RoleType> = {
  data: demoData,
  total: 1,
  params: {},
  allData: []
};

export const appUsersSlice = createSlice(
  {
    name: 'roles',
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
