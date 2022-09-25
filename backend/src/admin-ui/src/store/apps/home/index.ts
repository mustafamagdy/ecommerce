import {createSlice} from '@reduxjs/toolkit'

interface InitialState {
  dashboard?: any
}

const initialState: InitialState = {};

export const homeSlice = createSlice({
  name: 'home',
  initialState,
  reducers: {},
});

export default homeSlice.reducer;
