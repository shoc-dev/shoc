import { useContext } from "react";
import JobTasksContext, { JobTasksContextValueType } from "./job-tasks-context";

export default function useJobTasks(){
    return useContext<JobTasksContextValueType>(JobTasksContext);
}