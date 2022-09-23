// ** Redux Imports
import {Dispatch} from 'redux'
import {createSlice, createAsyncThunk} from '@reduxjs/toolkit'

// ** Axios Imports
import axios from 'axios'

interface InitialState {
  dashboard?: any
}

const initialState: InitialState = {};

export const homeSlice = createSlice({
  name: 'home',
  reducers: {},
  initialState: initialState
});

export default homeSlice.reducer;
