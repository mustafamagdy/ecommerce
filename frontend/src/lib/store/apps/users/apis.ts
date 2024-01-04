import {createAsyncThunk} from "@reduxjs/toolkit";
import {Dispatch} from "redux";
import {Http} from 'src/lib/services/http'
import {endPoints} from "src/lib/services/endpoints";

export interface DataParams {
  q?: string
  role?: string
  status?: string,
  pageNumber: number,
  pageSize: number
}

interface Redux {
  getState: any
  dispatch: Dispatch<any>
}

// ** Fetch Users
export const fetchData = createAsyncThunk('users/fetchData', async (params: DataParams) => {
  return await Http.post(endPoints.userList.url, params)
})

// ** Add User
export const addUser = createAsyncThunk('users/addUser',
  async (data: { [key: string]: number | string }, {getState, dispatch}: Redux) => {
    const response = await Http.post(endPoints.userList.url, {
      data
    })
    dispatch(fetchData(getState().user.params))
    return response.data
  }
)

// ** Delete User
export const deleteUser = createAsyncThunk('users/deleteUser',
  async (id: number | string, {getState, dispatch}: Redux) => {
    const response = await Http.delete(`${endPoints.userDelete.url}/${id}`)
    dispatch(fetchData(getState().user.params))
    return response.data
  }
)
