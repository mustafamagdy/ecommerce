import {createSlice} from '@reduxjs/toolkit'
import {fetchData} from "./apis";
import {InitialState} from "src/types/apps/generics";
import {PermissionRowType} from "src/types/apps/permissionTypes";


const initialState: InitialState<PermissionRowType> = {
  data: [],
  total: 1,
  params: {},
  allData: []
};

export const permissionsSlice = createSlice({
  name: 'permissions',
  initialState,
  reducers: {},
  extraReducers: builder => {
    builder.addCase(fetchData.fulfilled, (state, action) => {
      state.data = action.payload.permissions
      state.params = action.payload.params
      state.allData = action.payload.allData
      state.total = action.payload.total
    })
  }
})

export default permissionsSlice.reducer
