import {createAsyncThunk} from "@reduxjs/toolkit";
import axios from "axios";
import {Redux} from "src/types/apps/generics";


interface DataParams {
  q: string

}
