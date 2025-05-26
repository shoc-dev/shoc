import { JobTaskValueType } from "@/domain/job";
import { createContext } from "react";

const JobTasksContext = createContext<JobTasksContextValueType | any>({});

export type JobTasksContextValueType = {
    value: JobTaskValueType[],
    load: () => Promise<any>,
    loading: boolean,
    errors: any[]
}

export default JobTasksContext;