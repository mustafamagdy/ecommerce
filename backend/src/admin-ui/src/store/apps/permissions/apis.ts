import {createAsyncThunk} from "@reduxjs/toolkit";
import axios from "axios";


interface DataParams {
  q: string
}

// ** Fetch Invoices
export const fetchData = createAsyncThunk('permissions/fetchData', async (params: DataParams) => {
  const response = await axios.get('/apps/permissions/data', {
    params
  })

  return response.data
})
