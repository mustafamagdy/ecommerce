import {Dispatch} from "redux";

export interface Redux {
  getState: any
  dispatch: Dispatch<any>
}

export interface InitialState<T> {
  data: T[],
  total: number,
  params: any,
  allData: any[]
}
