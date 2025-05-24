import { useContext } from "react";
import JobContext, { JobContextValueType } from "./job-context";

export default function useJob(){
    return useContext<JobContextValueType>(JobContext);
}