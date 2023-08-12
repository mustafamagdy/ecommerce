import {Dispatch} from "redux";

export interface Redux {
  getState: any
  dispatch: Dispatch<any>
}

export interface InitialState<TData, TParams> {
  data: TData[],
  total: number,
  params: TParams
}
