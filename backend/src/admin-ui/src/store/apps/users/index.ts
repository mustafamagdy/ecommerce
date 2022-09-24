import {createSlice} from '@reduxjs/toolkit'
import {fetchData} from "./apis";
import {InitialState} from "src/types/apps/generics";
import {UserType} from "src/types/apps/userTypes";

const demo_roles = ['admin', 'supervisor', 'user', 'support'];
const demo_status = ['active', 'pending', 'inactive'];


const demoData = new Array(100)
  .fill(0)
  .map((_, i) => ({
    id: (i + 1).toString(),
    firstName: 'full',
    lastName: 'name',
    email: `email${i}@root.com`,
    active: Math.random() > .50,
    role: demo_roles[Math.floor(Math.random() * demo_roles.length)],
    userName: `username${i}`,
    emailConfirmed: Math.random() > .50,
    phoneNumber: '',
    imagePath: '',
    status: demo_status[Math.floor(Math.random() * demo_status.length)],
    fullName: `Full name ${i}`,
  }));

const initialState: InitialState<UserType> = {
  data: demoData,
  total: 1,
  params: {},
  allData: []
};

export const appUsersSlice = createSlice(
  {
    name: 'users',
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
