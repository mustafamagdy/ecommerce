import {createSlice} from '@reduxjs/toolkit'
import {fetchData} from "./apis";
import {InitialState} from "src/types/apps/generics";
import {UsersType} from "src/types/apps/userTypes";


const initialState: InitialState<UsersType> = {
  data: [],
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
