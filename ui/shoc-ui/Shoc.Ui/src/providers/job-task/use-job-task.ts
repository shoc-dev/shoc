import { useContext } from "react";
import JobTaskContext, { JobTaskContextValueType } from "./job-task-context";

export default function useJobTask(){
    return useContext<JobTaskContextValueType>(JobTaskContext);
}