import {createAsyncThunk} from "@reduxjs/toolkit";
import axios from "axios";
import {Dispatch} from "redux";


interface DataParams {
  q: string
  role: string
  status: string
}

interface Redux {
  getState: any
  dispatch: Dispatch<any>
}

// ** Fetch Users
export const fetchData = createAsyncThunk('users/fetchData', async (params: DataParams) => {
  const response = await axios.get('/apps/roles/list', {
    params
  })

  return response.data
})
