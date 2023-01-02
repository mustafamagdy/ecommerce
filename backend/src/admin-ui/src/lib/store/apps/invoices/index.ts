import {createSlice} from '@reduxjs/toolkit'
import {fetchData} from "./apis";
import {InitialState} from "src/types/apps/generics";
import {InvoiceType} from "src/types/apps/invoiceTypes";


const initialState: InitialState<InvoiceType> = {
  data: [],
  total: 1,
  params: {},
  allData: []
};

export const invoiceSlice = createSlice(
  {
    name: 'invoices',
    initialState,
    reducers: {},
    extraReducers: builder => {
      builder.addCase(fetchData.fulfilled, (state, action) => {
        state.data = action.payload.invoices
        state.params = action.payload.params
        state.allData = action.payload.allData
        state.total = action.payload.total
      })
    }
  })

export default invoiceSlice.reducer
