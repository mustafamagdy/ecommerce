import {createAsyncThunk} from "@reduxjs/toolkit";
import axios from "axios";
import {Redux} from "src/types/apps/generics";


interface DataParams {
  q: string
  dates?: Date[]
  status: string
}


// ** Fetch Invoices
export const fetchData = createAsyncThunk('appInvoice/fetchData', async (params: DataParams) => {
  const response = await axios.get('/apps/invoice/invoices', {
    params
  })

  return response.data
})

export const deleteInvoice = createAsyncThunk('appInvoice/deleteData',
  async (id: number | string, {getState, dispatch}: Redux) => {
    const response = await axios.delete('/apps/invoice/delete', {
      data: id
    })
    await dispatch(fetchData(getState().invoice.params))

    return response.data
  }
)
