import {createSlice} from '@reduxjs/toolkit'
import {fetchData} from "./apis";
import {InitialState} from "src/types/apps/generics";
import {AppUser} from "src/types/apps/userTypes";

const roles = ['admin', 'supervisor', 'user', 'support'];

const demoData = new Array(100)
  .fill(0)
  .map((_, i) => new AppUser(
    (i + 1).toString(),
    'full',
    'name',
    `email${i}@root.com`,
    Math.random() > .50,
    roles[Math.floor(Math.random() * roles.length)],
    `username${i}`,
    Math.random() > .50,
    '',
    ''
  ));

const initialState: InitialState<AppUser> = {
  data: demoData,
  total: 1,
  params: {},
  allData: []
};

export const appUsersSlice = createSlice(
  {
    name: 'appUsers',
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
