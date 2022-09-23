import {createSlice} from '@reduxjs/toolkit'

interface InitialState {
  dashboard?: any
}

const initialState: InitialState = {};

export const homeSlice = createSlice({
  name: 'home',
  reducers: {},
  initialState
});

export default homeSlice.reducer;
